namespace Education.API.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health")
            .WithTags("Health")
            .AllowAnonymous();

        return app;
    }
}
