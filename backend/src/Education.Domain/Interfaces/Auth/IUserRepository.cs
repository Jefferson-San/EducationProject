using Education.Domain.Entities.Auth;

namespace Education.Domain.Interfaces.Auth;

public interface IUserRepository : IRepository<User>
{
    /// <param name="normalizedEmail">E-mail já normalizado com <see cref="User.NormalizeEmail"/>.</param>
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
}
