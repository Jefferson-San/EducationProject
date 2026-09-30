namespace Education.Infrastructure.Authentication;

/// <summary>
/// Nomes das claims do JWT. Emitidas pelo TokenService e lidas pela validação do JWT (CrossCutting).
/// Com MapInboundClaims = false, chegam na API com os mesmos nomes.
/// </summary>
public static class AuthClaims
{
    public const string UserId = "sub";
    public const string Email = "email";
    public const string Name = "name";
    public const string Role = "role";
}
