namespace VideoProcessing.Processing;

public sealed record ProcessingResult(bool Success, TimeSpan? Duration, IReadOnlyList<string> Renditions, string? Error)
{
    public static ProcessingResult Ok(TimeSpan? duration, IReadOnlyList<string> renditions) =>
        new(true, duration, renditions, null);

    public static ProcessingResult Fail(string error) => new(false, null, [], error);
}

/// <summary>
/// Facade sobre o FFmpeg: o resto do Worker não monta comandos nem conhece argumentos do FFmpeg.
/// </summary>
public interface IFFmpegService
{
    /// <summary>
    /// Converte o vídeo em HLS dentro de <paramref name="outputFolder"/>:
    /// master.m3u8 + uma pasta por qualidade (playlist.m3u8 + segmentos .ts).
    /// </summary>
    Task<ProcessingResult> ConvertToHlsAsync(string inputPath, string outputFolder, CancellationToken cancellationToken);
}
