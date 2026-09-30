using Education.Domain.Common;
using Education.Domain.Interfaces.Auth;
using MediatR;

namespace Education.Application.Auth.Commands;

/// <summary>
/// Refresh Token Rotation: valida A → revoga A → gera B + novo Access Token.
/// Reapresentar um token já rotacionado revoga todos os tokens ativos do usuário (possível vazamento).
/// </summary>
public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly AuthTokenIssuer _tokenIssuer;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        ITokenService tokens,
        AuthTokenIssuer tokenIssuer)
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _tokens = tokens;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var stored = await _refreshTokens.GetByHashAsync(_tokens.HashRefreshToken(request.RefreshToken), cancellationToken);

        if (stored is null)
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token inválido."));

        if (stored.IsExpired(now))
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.ExpiredRefreshToken", "Refresh token expirado."));

        if (stored.Revoke(now).IsFailure)
            return await RevokeAllAsync(stored.UserId, now, cancellationToken);

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<AuthResponse>(Error.Unauthorized("Auth.InvalidRefreshToken", "Refresh token inválido."));

        try
        {
            // Grava a revogação de A junto com a criação de B.
            return Result.Success(await _tokenIssuer.IssueAsync(user, cancellationToken));
        }
        catch (ConcurrencyException)
        {
            // Duas requisições com o mesmo token ao mesmo tempo: só a primeira vence; a outra é tratada como reuso.
            return await RevokeAllAsync(stored.UserId, now, cancellationToken);
        }
    }

    private async Task<Result<AuthResponse>> RevokeAllAsync(Guid userId, DateTime now, CancellationToken cancellationToken)
    {
        await _refreshTokens.RevokeAllActiveAsync(userId, now, cancellationToken);
        return Result.Failure<AuthResponse>(Error.Unauthorized(
            "Auth.RefreshTokenReused", "Refresh token já utilizado. Faça login novamente."));
    }
}
