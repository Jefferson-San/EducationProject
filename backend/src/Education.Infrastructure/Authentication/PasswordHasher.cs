using Education.Domain.Entities.Auth;
using Education.Domain.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;

namespace Education.Infrastructure.Authentication;

/// <summary>
/// Adapter sobre o PasswordHasher do ASP.NET Core Identity (PBKDF2 com salt, formato versionado).
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();

    // O hasher do Identity não usa o usuário no cálculo; o parâmetro existe só por causa da assinatura genérica.
    public string Hash(string password) => _inner.HashPassword(null!, password);

    public PasswordVerification Verify(string passwordHash, string password) =>
        _inner.VerifyHashedPassword(null!, passwordHash, password) switch
        {
            PasswordVerificationResult.Success => PasswordVerification.Success,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordVerification.SuccessRehashNeeded,
            _ => PasswordVerification.Failed
        };
}
