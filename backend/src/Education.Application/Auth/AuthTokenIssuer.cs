using Education.Domain.Entities.Auth;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Auth;

namespace Education.Application.Auth;

/// <summary>
/// Emite o par Access Token + Refresh Token (cadastro, login e refresh) e grava apenas o hash do refresh token.
/// O SaveChanges aqui também confirma as alterações pendentes de quem chamou (ex.: revogação do token anterior).
/// </summary>
public sealed class AuthTokenIssuer
{
    private readonly ITokenService _tokens;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public AuthTokenIssuer(ITokenService tokens, IRefreshTokenRepository refreshTokens, IUnitOfWork unitOfWork)
    {
        _tokens = tokens;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> IssueAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _tokens.CreateAccessToken(user);
        var refreshToken = _tokens.CreateRefreshToken();

        await _refreshTokens.AddAsync(
            RefreshToken.Create(user.Id, refreshToken.TokenHash, refreshToken.ExpiresAt), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken.Token, accessToken.ExpiresAt, refreshToken.Token, refreshToken.ExpiresAt,
            user.ToDto());
    }
}
