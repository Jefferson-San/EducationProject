using Education.Domain.Common;

namespace Education.Domain.Entities.Auth;

public class User : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    /// <param name="passwordHash">Hash já calculado; a senha pura nunca chega ao domínio.</param>
    public static User Create(string name, string email, string passwordHash, UserRole role) => new()
    {
        Name = name.Trim(),
        Email = NormalizeEmail(email),
        PasswordHash = passwordHash,
        Role = role,
        CreatedAt = DateTime.UtcNow
    };

    /// <summary>Formato único de e-mail usado no cadastro, no login e na checagem de duplicidade.</summary>
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>Usado quando o formato do hash evolui (rehash transparente no login).</summary>
    public void UpdatePasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
