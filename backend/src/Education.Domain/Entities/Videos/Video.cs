using Education.Domain.Common;
using Education.Domain.Events;

namespace Education.Domain.Entities.Videos;

/// <summary>
/// Vídeo de uma aula (1:1) e sua máquina de estados:
/// Uploaded → Processing → Processed, ou Processing → Failed. Failed pode voltar a Processing (novo envio da fila).
/// </summary>
public class Video : AggregateRoot
{
    private const int MaxFailureReasonLength = 1000;

    public Guid LessonId { get; private set; }

    /// <summary>Chave relativa no Storage, ex.: "original/{id}/original.mp4".</summary>
    public string OriginalPath { get; private set; } = default!;

    /// <summary>Chave relativa do master playlist, ex.: "processed/{id}/master.m3u8".</summary>
    public string? StreamingPath { get; private set; }

    public VideoStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>Token de concorrência otimista (mapeado para o xmin do PostgreSQL).</summary>
    public uint Version { get; private set; }

    private Video() { }

    private Video(Guid id) : base(id) { }

    /// <param name="id">Gerado antes, porque a chave do arquivo no Storage depende dele.</param>
    public static Video Create(Guid id, Guid lessonId, string originalPath)
    {
        var video = new Video(id)
        {
            LessonId = lessonId,
            OriginalPath = originalPath,
            Status = VideoStatus.Uploaded,
            CreatedAt = DateTime.UtcNow
        };
        video.RaiseDomainEvent(new VideoUploadedEvent(video.Id, video.OriginalPath));
        return video;
    }

    /// <param name="redelivered">
    /// Mensagem reentregue pela fila: se o vídeo ainda está em Processing, o Worker anterior caiu no meio
    /// e é seguro reprocessar. Sem reentrega, Processing significa mensagem duplicada.
    /// </param>
    public Result StartProcessing(bool redelivered)
    {
        var allowed = Status is VideoStatus.Uploaded or VideoStatus.Failed
            || (Status == VideoStatus.Processing && redelivered);

        if (!allowed)
            return InvalidTransition("iniciar o processamento");

        Status = VideoStatus.Processing;
        FailureReason = null;
        return Result.Success();
    }

    public Result Complete(string streamingPath, TimeSpan? duration)
    {
        if (Status != VideoStatus.Processing)
            return InvalidTransition("concluir o processamento");

        Status = VideoStatus.Processed;
        StreamingPath = streamingPath;
        Duration = duration is { TotalSeconds: > 0 } ? duration : null;
        return Result.Success();
    }

    public Result Fail(string reason)
    {
        if (Status != VideoStatus.Processing)
            return InvalidTransition("registrar falha no processamento");

        Status = VideoStatus.Failed;
        FailureReason = Truncate(reason);
        return Result.Success();
    }

    /// <summary>O vídeo nem chegou à fila (ex.: RabbitMQ indisponível): o professor precisa reenviar.</summary>
    public Result FailToEnqueue(string reason)
    {
        if (Status != VideoStatus.Uploaded)
            return InvalidTransition("registrar falha no envio para processamento");

        Status = VideoStatus.Failed;
        FailureReason = Truncate(reason);
        return Result.Success();
    }

    /// <summary>Enquanto processa, o vídeo não pode ser substituído (o Worker está usando o arquivo).</summary>
    public bool CanBeReplaced => Status != VideoStatus.Processing;

    private Result InvalidTransition(string action) =>
        Result.Failure(Error.Conflict("Video.InvalidTransition",
            $"Não é possível {action}: o vídeo está {Status}."));

    private static string Truncate(string reason)
    {
        var text = string.IsNullOrWhiteSpace(reason) ? "Falha no processamento." : reason.Trim();
        return text.Length > MaxFailureReasonLength ? text[..MaxFailureReasonLength] : text;
    }
}
