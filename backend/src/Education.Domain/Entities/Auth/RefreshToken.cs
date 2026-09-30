using Education.Domain.Common;

namespace Education.Domain.Entities.Auth;

public class RefreshToken : AggregateRoot
{
    public Guid UserId { get; private set; }

    /// <summary>Hash (SHA-256) do token. O token puro nunca é armazenado.</summary>
    public string TokenHash { get; private set; } = default!;

    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    /// <summary>Token de concorrência otimista (mapeado para o xmin do PostgreSQL).</summary>
    public uint Version { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt) => new()
    {
        UserId = userId,
        TokenHash = tokenHash,
        ExpiresAt = expiresAt,
        CreatedAt = DateTime.UtcNow
    };

    public bool IsExpired(DateTime now) => ExpiresAt <= now;

    /// <summary>
    /// Revoga o token (rotação ou logout). Um token já revogado sendo usado de novo indica possível vazamento.
    /// </summary>
    public Result Revoke(DateTime now)
    {
        if (IsRevoked)
            return Result.Failure(Error.Conflict("RefreshToken.AlreadyRevoked", "Refresh token já utilizado."));

        RevokedAt = now;
        return Result.Success();
    }
}
