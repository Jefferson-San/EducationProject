namespace VideoProcessing.Processing;

public class FFmpegOptions
{
    public const string SectionName = "FFmpeg";

    public string FFmpegPath { get; set; } = "ffmpeg";
    public string FFprobePath { get; set; } = "ffprobe";

    /// <summary>Velocidade x compressão do x264. "veryfast" é um bom equilíbrio para rodar num notebook na demo.</summary>
    public string Preset { get; set; } = "veryfast";

    /// <summary>Duração alvo de cada segmento .ts.</summary>
    public int SegmentSeconds { get; set; } = 6;

    /// <summary>Tempo máximo de uma conversão. Deve ser menor que o consumer_timeout do RabbitMQ (2 h no compose).</summary>
    public int TimeoutMinutes { get; set; } = 90;
}
