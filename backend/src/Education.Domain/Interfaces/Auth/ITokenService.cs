using Education.Domain.Entities.Auth;

namespace Education.Domain.Interfaces.Auth;

public sealed record AccessToken(string Token, DateTime ExpiresAt);

/// <param name="Token">Valor puro, entregue apenas ao cliente.</param>
/// <param name="TokenHash">Hash do token — é o que vai para o banco.</param>
public sealed record NewRefreshToken(string Token, string TokenHash, DateTime ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);

    NewRefreshToken CreateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
