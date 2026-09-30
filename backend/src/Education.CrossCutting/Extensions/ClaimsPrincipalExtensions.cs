using System.Security.Claims;
using Education.Infrastructure.Authentication;

namespace Education.CrossCutting.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id do usuário autenticado (claim "sub" do JWT).</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(AuthClaims.UserId)
            ?? throw new InvalidOperationException("Token sem a claim 'sub'."));
}
