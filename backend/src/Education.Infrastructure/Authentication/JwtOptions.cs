using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Education.Infrastructure.Authentication;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "education-api";
    public string Audience { get; set; } = "education-frontend";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;

    public SymmetricSecurityKey GetSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));

    public void Validate()
    {
        // HMAC-SHA256 exige chave de pelo menos 256 bits.
        if (Encoding.UTF8.GetByteCount(SigningKey) < 32)
            throw new InvalidOperationException("Jwt:SigningKey deve ter pelo menos 32 caracteres.");
    }
}
