using System.Security.Claims;
using Education.Application.Courses.Commands;
using Education.Application.Courses.Queries;
using Education.CrossCutting.Authentication;
using Education.CrossCutting.Extensions;
using MediatR;

namespace Education.API.Endpoints;

public static class CoursesEndpoints
{
    public static IEndpointRouteBuilder MapCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/courses")
            .WithTags("Cursos")
            .RequireAuthorization();

        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCoursesQuery(), ct);
            return result.ToApiResult();
        })
        .WithName("ListarCursos")
        .WithSummary("Lista todos os cursos (MVP sem matrícula)");

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetCourseByIdQuery(id), ct);
            return result.ToApiResult();
        })
        .WithName("ObterCurso")
        .WithSummary("Busca um curso com as aulas em ordem");

        group.MapPost("/", async (
            CreateCourseRequest request,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new CreateCourseCommand(user.GetUserId(), request.Title, request.Description), ct);

            return result.IsSuccess
                ? Results.Created($"/v1/courses/{result.Value.Id}", result.Value)
                : result.ToApiResult();
        })
        .RequireAuthorization(AuthorizationPolicies.Teacher)
        .WithName("CriarCurso")
        .WithSummary("Cria um curso (somente Teacher)");

        group.MapPost("/{courseId:guid}/lessons", async (
            Guid courseId,
            CreateLessonRequest request,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(
                new CreateLessonCommand(user.GetUserId(), courseId, request.Title, request.Description, request.Order), ct);

            return result.IsSuccess
                ? Results.Created($"/v1/lessons/{result.Value.Id}", result.Value)
                : result.ToApiResult();
        })
        .RequireAuthorization(AuthorizationPolicies.Teacher)
        .WithName("CriarAula")
        .WithSummary("Cria uma aula no curso (somente o Teacher dono do curso)");

        return app;
    }
}

public sealed record CreateCourseRequest(string Title, string? Description);

public sealed record CreateLessonRequest(string Title, string? Description, int? Order);
