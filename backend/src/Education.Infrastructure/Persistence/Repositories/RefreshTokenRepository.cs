using Education.Domain.Entities.Auth;
using Education.Domain.Interfaces.Auth;
using Microsoft.EntityFrameworkCore;

namespace Education.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    /// <summary>UPDATE direto no banco (sem carregar os tokens), aplicado imediatamente.</summary>
    public async Task RevokeAllActiveAsync(Guid userId, DateTime now, CancellationToken cancellationToken = default) =>
        await DbSet
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevokedAt, now), cancellationToken);
}
