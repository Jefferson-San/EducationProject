namespace VideoProcessing.Processing;

/// <summary>Uma qualidade do HLS. Height é o lado menor do vídeo (360 = 640x360 em paisagem, 360x640 em retrato).</summary>
public sealed record HlsRendition(string Name, int Height, int VideoKbps, int AudioKbps)
{
    public static readonly IReadOnlyList<HlsRendition> Defaults =
    [
        new("360p", 360, VideoKbps: 800, AudioKbps: 96),
        new("480p", 480, VideoKbps: 1400, AudioKbps: 128),
        new("720p", 720, VideoKbps: 2800, AudioKbps: 128)
    ];

    /// <summary>
    /// Qualidades até a resolução do original (sem upscale: um vídeo 480p não gera 720p).
    /// Vídeos menores que 360p geram uma única qualidade na resolução original.
    /// </summary>
    public static IReadOnlyList<HlsRendition> SelectFor(int sourceShortSide)
    {
        var fitting = Defaults.Where(r => r.Height <= sourceShortSide).ToList();
        if (fitting.Count > 0) return fitting;

        var evenHeight = Math.Max(2, sourceShortSide - sourceShortSide % 2); // H.264 exige dimensões pares
        return [Defaults[0] with { Name = $"{evenHeight}p", Height = evenHeight }];
    }
}
