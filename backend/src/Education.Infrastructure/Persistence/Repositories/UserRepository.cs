using Education.Domain.Entities.Auth;
using Education.Domain.Interfaces.Auth;
using Microsoft.EntityFrameworkCore;

namespace Education.Infrastructure.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        await DbSet.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
}
