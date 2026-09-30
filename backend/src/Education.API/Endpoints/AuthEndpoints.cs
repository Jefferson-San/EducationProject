using System.Security.Claims;
using Education.Application.Auth.Commands;
using Education.CrossCutting.Extensions;
using MediatR;

namespace Education.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/v1/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Created((string?)null, result.Value)
                : result.ToApiResult();
        })
        .AllowAnonymous()
        .WithName("Cadastrar")
        .WithSummary("Cadastra um usuário (Teacher ou Student) e já retorna os tokens");

        group.MapPost("/login", async (
            LoginCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToApiResult();
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Autentica com e-mail e senha e retorna access token + refresh token");

        group.MapPost("/refresh", async (
            RefreshTokenCommand command,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToApiResult();
        })
        .AllowAnonymous()
        .WithName("RenovarToken")
        .WithSummary("Troca o refresh token por um novo par de tokens (rotação)");

        group.MapPost("/logout", async (
            LogoutRequest request,
            ClaimsPrincipal user,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new LogoutCommand(user.GetUserId(), request.RefreshToken), ct);
            return result.ToApiResult();
        })
        .RequireAuthorization()
        .WithName("Logout")
        .WithSummary("Revoga o refresh token do usuário autenticado");

        return app;
    }
}

public sealed record LogoutRequest(string RefreshToken);
