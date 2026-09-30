using Education.Domain.Entities.Auth;

namespace Education.Domain.Interfaces.Auth;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Revoga todos os refresh tokens ativos do usuário (resposta a um possível vazamento).</summary>
    Task RevokeAllActiveAsync(Guid userId, DateTime now, CancellationToken cancellationToken = default);
}
