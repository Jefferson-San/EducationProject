using System.Net;
using System.Net.Http.Json;
using VideoProcessing.Contracts.Internal;

namespace VideoProcessing.Api;

/// <summary>
/// Cliente HTTP tipado. BaseAddress e o header X-Internal-Api-Key são configurados no Program.cs.
/// </summary>
public sealed class EducationApiClient(HttpClient http, ILogger<EducationApiClient> logger) : IEducationApiClient
{
    public async Task<StartProcessingResult> StartAsync(Guid videoId, bool redelivered,
        CancellationToken cancellationToken)
    {
        using var response = await http.PostAsync(
            $"internal/videos/{videoId}/processing/start?redelivered={redelivered.ToString().ToLowerInvariant()}",
            content: null, cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.NoContent => StartProcessingResult.Started,
            HttpStatusCode.Conflict => StartProcessingResult.AlreadyHandled,
            HttpStatusCode.NotFound => StartProcessingResult.NotFound,
            _ => throw await UnexpectedAsync(response, "start", cancellationToken)
        };
    }

    public async Task CompleteAsync(Guid videoId, string streamingPath, TimeSpan? duration,
        CancellationToken cancellationToken)
    {
        using var response = await http.PostAsJsonAsync($"internal/videos/{videoId}/processing/complete",
            new CompleteProcessingRequest(streamingPath, duration?.TotalSeconds), cancellationToken);

        if (response.StatusCode != HttpStatusCode.NoContent)
            throw await UnexpectedAsync(response, "complete", cancellationToken);
    }

    public async Task FailAsync(Guid videoId, string reason, CancellationToken cancellationToken)
    {
        using var response = await http.PostAsJsonAsync($"internal/videos/{videoId}/processing/fail",
            new FailProcessingRequest(reason), cancellationToken);

        // 409 = o vídeo já não está em Processing (ex.: já foi concluído); não há o que fazer.
        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            logger.LogWarning("API recusou marcar falha do vídeo {VideoId} (409)", videoId);
            return;
        }

        if (response.StatusCode != HttpStatusCode.NoContent)
            throw await UnexpectedAsync(response, "fail", cancellationToken);
    }

    private static async Task<HttpRequestException> UnexpectedAsync(HttpResponseMessage response, string action,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new HttpRequestException(
            $"Education API respondeu {(int)response.StatusCode} no '{action}': {body}", null, response.StatusCode);
    }
}
