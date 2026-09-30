using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Education.Domain.Entities.Auth;
using Education.Domain.Interfaces.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Education.Infrastructure.Authentication;

public sealed class TokenService : ITokenService
{
    private readonly IOptions<JwtOptions> _options;
    private readonly JsonWebTokenHandler _handler = new();

    public TokenService(IOptions<JwtOptions> options) => _options = options;

    public AccessToken CreateAccessToken(User user)
    {
        var jwt = _options.Value;
        var expiresAt = DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Expires = expiresAt,
            Claims = new Dictionary<string, object>
            {
                [AuthClaims.UserId] = user.Id.ToString(),
                [AuthClaims.Email] = user.Email,
                [AuthClaims.Name] = user.Name,
                [AuthClaims.Role] = user.Role.ToString()
            },
            SigningCredentials = new SigningCredentials(jwt.GetSigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }

    public NewRefreshToken CreateRefreshToken()
    {
        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));
        return new NewRefreshToken(token, HashRefreshToken(token),
            DateTime.UtcNow.AddDays(_options.Value.RefreshTokenDays));
    }

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
