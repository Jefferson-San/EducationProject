// Contrato duplicado em Education.API/Endpoints/InternalVideosEndpoints.cs e Education.CrossCutting/Authentication/InternalApiKeyAuthenticationHandler.cs: qualquer mudança aqui deve ser feita lá também.
namespace VideoProcessing.Contracts.Internal;

/// <summary>
/// Contrato das chamadas Worker → Education API (rotas /internal/videos/{id}/processing/*).
/// </summary>
public static class InternalApi
{
    public const string KeyHeader = "X-Internal-Api-Key";
}

public sealed record CompleteProcessingRequest(string StreamingPath, double? DurationSeconds);

public sealed record FailProcessingRequest(string Reason);
