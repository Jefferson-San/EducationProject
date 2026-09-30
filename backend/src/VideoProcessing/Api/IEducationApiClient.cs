namespace VideoProcessing.Api;

public enum StartProcessingResult
{
    /// <summary>Status mudou para Processing: pode processar.</summary>
    Started,

    /// <summary>409: já processado ou em processamento por outra entrega (mensagem duplicada).</summary>
    AlreadyHandled,

    /// <summary>404: vídeo não existe mais (ex.: professor substituiu o vídeo antes do processamento).</summary>
    NotFound
}

/// <summary>Chamadas Worker → Education API. O Worker não acessa o banco diretamente.</summary>
public interface IEducationApiClient
{
    Task<StartProcessingResult> StartAsync(Guid videoId, bool redelivered, CancellationToken cancellationToken);

    Task CompleteAsync(Guid videoId, string streamingPath, TimeSpan? duration, CancellationToken cancellationToken);

    Task FailAsync(Guid videoId, string reason, CancellationToken cancellationToken);
}
