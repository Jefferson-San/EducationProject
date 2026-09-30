using System.Security.Claims;
using Education.Application.Lessons.Queries;
using Education.Application.Videos.Commands;
using Education.CrossCutting.Authentication;
using Education.CrossCutting.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Education.API.Endpoints;

public static class LessonsEndpoints
{
    public static IEndpointRouteBuilder MapLessonsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/lessons")
            .WithTags("Aulas")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetLessonByIdQuery(id), ct);
            return result.ToApiResult();
        })
        .WithName("ObterAula")
        .WithSummary("Busca a aula com o status do vídeo e, quando pronto, a URL do HLS (usado também para polling)");

        group.MapPost("/{lessonId:guid}/video", async (
            Guid lessonId,
            IFormFile file,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken ct) =>
        {
            await using var content = file.OpenReadStream();
            var result = await mediator.Send(
                new UploadVideoCommand(user.GetUserId(), lessonId, file.FileName, file.Length, content), ct);

            return result.IsSuccess
                ? Results.Accepted($"/v1/lessons/{lessonId}", result.Value)
                : result.ToApiResult();
        })
        .RequireAuthorization(AuthorizationPolicies.Teacher)
        .DisableAntiforgery() // API com JWT no header, não usa cookies: antiforgery não se aplica.
        .WithMetadata(new RequestSizeLimitAttribute(UploadVideoCommand.MaxFileSizeBytes + 1024 * 1024)) // + envelope multipart
        .WithFormOptions(multipartBodyLengthLimit: UploadVideoCommand.MaxFileSizeBytes)
        .WithName("EnviarVideo")
        .WithSummary("Envia (ou substitui) o vídeo .mp4 da aula, até 500 MB. O processamento é assíncrono (202)");

        return app;
    }
}
