using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Education.CrossCutting.Errors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Education.CrossCutting.Authentication;

/// <summary>
/// Autentica o Video Processing Worker pelo header X-Internal-Api-Key (chave compartilhada via configuração).
/// Usado só nos endpoints /internal/*, através da política <see cref="AuthorizationPolicies.InternalApi"/>.
/// </summary>
public class InternalApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "InternalApiKey";
    public const string HeaderName = "X-Internal-Api-Key";

    private readonly IOptions<InternalApiOptions> _internalApi;

    public InternalApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<InternalApiOptions> internalApi)
        : base(options, logger, encoder) => _internalApi = internalApi;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var providedKey))
            return Task.FromResult(AuthenticateResult.Fail("Internal API key not provided."));

        // Comparação em tempo constante para não vazar a chave por timing.
        var valid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedKey.ToString()),
            Encoding.UTF8.GetBytes(_internalApi.Value.Key));

        if (!valid)
            return Task.FromResult(AuthenticateResult.Fail("Invalid internal API key."));

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "video-processing")], SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        return Response.WriteAsJsonAsync(
            new ApiErrorResponse(401, "Chave interna ausente ou inválida.", "Auth.Unauthorized"));
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        return Response.WriteAsJsonAsync(
            new ApiErrorResponse(403, "Você não tem permissão para esta ação.", "Auth.Forbidden"));
    }
}
