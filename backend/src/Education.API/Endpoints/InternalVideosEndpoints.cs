using Education.Application.Videos.Commands;
using Education.CrossCutting.Authentication;
using Education.CrossCutting.Extensions;
using MediatR;

namespace Education.API.Endpoints;

/// <summary>
/// Endpoints chamados apenas pelo Video Processing Worker (header X-Internal-Api-Key). Fora do OpenAPI público.
/// Rotas sem versão: são contrato entre serviços, não API pública.
/// </summary>
public static class InternalVideosEndpoints
{
    public static IEndpointRouteBuilder MapInternalVideosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/internal/videos/{id:guid}/processing")
            .RequireAuthorization(AuthorizationPolicies.InternalApi)
            .ExcludeFromDescription();

        // UPLOADED/FAILED → PROCESSING. 409 = já processado ou em processamento (o Worker descarta a mensagem).
        group.MapPost("/start", async (
            Guid id,
            bool? redelivered,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new StartVideoProcessingCommand(id, redelivered ?? false), ct);
            return result.ToApiResult();
        });

        group.MapPost("/complete", async (
            Guid id,
            CompleteProcessingRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new CompleteVideoProcessingCommand(id, request.StreamingPath, request.DurationSeconds), ct);
            return result.ToApiResult();
        });

        group.MapPost("/fail", async (
            Guid id,
            FailProcessingRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new FailVideoProcessingCommand(id, request.Reason), ct);
            return result.ToApiResult();
        });

        return app;
    }
}

// Contrato duplicado em VideoProcessing/Contracts/Internal: qualquer mudança aqui deve ser feita lá também.
public sealed record CompleteProcessingRequest(string StreamingPath, double? DurationSeconds);

public sealed record FailProcessingRequest(string Reason);
