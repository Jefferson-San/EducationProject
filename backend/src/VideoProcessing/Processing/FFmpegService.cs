using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace VideoProcessing.Processing;

public sealed class FFmpegService(IOptions<FFmpegOptions> options, ILogger<FFmpegService> logger) : IFFmpegService
{
    private const string MasterPlaylist = "master.m3u8";

    public async Task<ProcessingResult> ConvertToHlsAsync(string inputPath, string outputFolder,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;

        var probe = await ProbeAsync(inputPath, cancellationToken);
        if (probe is null)
            return ProcessingResult.Fail("Arquivo de vídeo inválido ou corrompido (ffprobe não conseguiu ler).");
        if (probe.Width <= 0 || probe.Height <= 0)
            return ProcessingResult.Fail("O arquivo não contém uma trilha de vídeo.");

        var renditions = HlsRendition.SelectFor(Math.Min(probe.Width, probe.Height));
        logger.LogInformation("Convertendo {Width}x{Height} (áudio: {HasAudio}, duração: {Duration}) para {Renditions}",
            probe.Width, probe.Height, probe.HasAudio, probe.Duration, string.Join(", ", renditions.Select(r => r.Name)));

        Directory.CreateDirectory(outputFolder);
        var arguments = BuildHlsArguments(inputPath, renditions, probe, settings);

        var run = await ProcessRunner.RunAsync(settings.FFmpegPath, arguments, outputFolder,
            TimeSpan.FromMinutes(settings.TimeoutMinutes), cancellationToken);

        if (run.TimedOut)
            return ProcessingResult.Fail($"FFmpeg excedeu o tempo limite de {settings.TimeoutMinutes} minutos.");
        if (run.ExitCode != 0)
            return ProcessingResult.Fail($"FFmpeg falhou (código {run.ExitCode}): {run.ErrorTail}");
        if (!File.Exists(Path.Combine(outputFolder, MasterPlaylist)))
            return ProcessingResult.Fail("FFmpeg terminou sem gerar o master.m3u8.");

        return ProcessingResult.Ok(probe.Duration, renditions.Select(r => r.Name).ToList());
    }

    /// <summary>
    /// Um único comando gera todas as qualidades: o vídeo é decodificado uma vez e dividido (split)
    /// em N saídas redimensionadas. Os keyframes são forçados a cada SegmentSeconds para que os
    /// segmentos de todas as qualidades fiquem alinhados (o player troca de qualidade sem engasgar).
    /// Os caminhos de saída são relativos: o processo roda com o diretório de trabalho = pasta de saída.
    /// </summary>
    private static List<string> BuildHlsArguments(string inputPath, IReadOnlyList<HlsRendition> renditions,
        ProbeResult probe, FFmpegOptions settings)
    {
        var landscape = probe.Width >= probe.Height;
        var count = renditions.Count;

        // ex.: [0:v]split=3[s0][s1][s2];[s0]scale=-2:360[v0];[s1]scale=-2:480[v1];[s2]scale=-2:720[v2]
        var scales = renditions.Select((r, i) =>
            $"[s{i}]scale={(landscape ? $"-2:{r.Height}" : $"{r.Height}:-2")}[v{i}]");
        var filter = count == 1
            ? $"[0:v]scale={(landscape ? $"-2:{renditions[0].Height}" : $"{renditions[0].Height}:-2")}[v0]"
            : $"[0:v]split={count}{string.Concat(Enumerable.Range(0, count).Select(i => $"[s{i}]"))};{string.Join(';', scales)}";

        var args = new List<string>
        {
            "-hide_banner", "-nostdin", "-loglevel", "warning", "-y",
            "-i", inputPath,
            "-filter_complex", filter
        };

        for (var i = 0; i < count; i++) args.AddRange(["-map", $"[v{i}]"]);
        if (probe.HasAudio)
            for (var i = 0; i < count; i++) args.AddRange(["-map", "0:a:0"]);

        args.AddRange(
        [
            "-c:v", "libx264", "-preset", settings.Preset, "-pix_fmt", "yuv420p",
            "-force_key_frames", $"expr:gte(t,n_forced*{settings.SegmentSeconds})", "-sc_threshold", "0"
        ]);
        for (var i = 0; i < count; i++)
        {
            var r = renditions[i];
            args.AddRange(
            [
                $"-b:v:{i}", $"{r.VideoKbps}k",
                $"-maxrate:v:{i}", $"{(int)(r.VideoKbps * 1.07)}k",
                $"-bufsize:v:{i}", $"{r.VideoKbps * 2}k"
            ]);
        }

        if (probe.HasAudio)
        {
            args.AddRange(["-c:a", "aac", "-ac", "2"]);
            for (var i = 0; i < count; i++) args.AddRange([$"-b:a:{i}", $"{renditions[i].AudioKbps}k"]);
        }

        // ex.: "v:0,a:0,name:360p v:1,a:1,name:480p" → pastas 360p/ e 480p/
        var streamMap = string.Join(' ', renditions.Select((r, i) =>
            probe.HasAudio ? $"v:{i},a:{i},name:{r.Name}" : $"v:{i},name:{r.Name}"));

        args.AddRange(
        [
            "-f", "hls",
            "-hls_time", settings.SegmentSeconds.ToString(CultureInfo.InvariantCulture),
            "-hls_playlist_type", "vod",
            "-hls_flags", "independent_segments",
            "-hls_segment_filename", "%v/segment%03d.ts",
            "-master_pl_name", MasterPlaylist,
            "-var_stream_map", streamMap,
            "%v/playlist.m3u8"
        ]);

        return args;
    }

    private sealed record ProbeResult(int Width, int Height, bool HasAudio, TimeSpan? Duration);

    private async Task<ProbeResult?> ProbeAsync(string inputPath, CancellationToken cancellationToken)
    {
        var run = await ProcessRunner.RunAsync(options.Value.FFprobePath,
            ["-v", "error", "-print_format", "json",
             "-show_entries", "stream=codec_type,width,height:format=duration", inputPath],
            workingDirectory: null, TimeSpan.FromMinutes(2), cancellationToken);

        if (run.TimedOut || run.ExitCode != 0)
        {
            logger.LogWarning("ffprobe falhou para {Input}: {Error}", inputPath, run.ErrorTail);
            return null;
        }

        using var json = JsonDocument.Parse(run.StandardOutput);
        var root = json.RootElement;

        int width = 0, height = 0;
        var hasAudio = false;
        if (root.TryGetProperty("streams", out var streams))
        {
            foreach (var stream in streams.EnumerateArray())
            {
                var type = stream.GetProperty("codec_type").GetString();
                if (type == "video" && width == 0)
                {
                    width = stream.TryGetProperty("width", out var w) ? w.GetInt32() : 0;
                    height = stream.TryGetProperty("height", out var h) ? h.GetInt32() : 0;
                }
                else if (type == "audio")
                {
                    hasAudio = true;
                }
            }
        }

        TimeSpan? duration = null;
        if (root.TryGetProperty("format", out var format)
            && format.TryGetProperty("duration", out var d)
            && double.TryParse(d.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            duration = TimeSpan.FromSeconds(seconds);
        }

        return new ProbeResult(width, height, hasAudio, duration);
    }
}
