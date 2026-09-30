namespace Education.Domain.Interfaces.Auth;

public enum PasswordVerification
{
    Failed,
    Success,

    /// <summary>Senha correta, mas o hash usa um formato antigo e deve ser regravado.</summary>
    SuccessRehashNeeded
}

public interface IPasswordHasher
{
    string Hash(string password);
    PasswordVerification Verify(string passwordHash, string password);
}
