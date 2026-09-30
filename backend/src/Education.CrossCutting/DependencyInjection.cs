using Education.CrossCutting.Authentication;
using Education.CrossCutting.Errors;
using Education.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Education.CrossCutting;

public static class DependencyInjection
{
    public static IServiceCollection AddCrossCutting(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        jwt.Validate();

        var internalApi = configuration.GetSection(InternalApiOptions.SectionName).Get<InternalApiOptions>()
            ?? new InternalApiOptions();
        internalApi.Validate();
        services.Configure<InternalApiOptions>(configuration.GetSection(InternalApiOptions.SectionName));

        // JWT (usuários) é o esquema padrão; a chave interna vale só para os endpoints do Worker.
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = jwt.GetSigningKey(),
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = AuthClaims.Name,
                    RoleClaimType = AuthClaims.Role
                };

                // Mesmo formato de erro (ApiErrorResponse) do resto da API.
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return context.Response.WriteAsJsonAsync(new ApiErrorResponse(
                            401, "Token ausente, inválido ou expirado.", "Auth.Unauthorized"));
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return context.Response.WriteAsJsonAsync(new ApiErrorResponse(
                            403, "Você não tem permissão para esta ação.", "Auth.Forbidden"));
                    }
                };
            })
            .AddScheme<AuthenticationSchemeOptions, InternalApiKeyAuthenticationHandler>(
                InternalApiKeyAuthenticationHandler.SchemeName, _ => { });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.Teacher, policy => policy.RequireRole("Teacher"))
            .AddPolicy(AuthorizationPolicies.InternalApi, policy => policy
                .AddAuthenticationSchemes(InternalApiKeyAuthenticationHandler.SchemeName)
                .RequireAuthenticatedUser());

        return services;
    }
}
