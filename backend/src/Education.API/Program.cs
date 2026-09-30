using System.Text.Json;
using System.Text.Json.Serialization;
using Education.API.Endpoints;
using Education.API.OpenApi;
using Education.Application;
using Education.CrossCutting;
using Education.CrossCutting.Middleware;
using Education.Infrastructure;
using Education.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddCrossCutting(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

const string CorsPolicyName = "Frontend";
var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
        policy.WithOrigins(corsAllowedOrigins)
            .WithHeaders("Content-Type", "Authorization")
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE"));
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Education API v1"));

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapCoursesEndpoints();
app.MapLessonsEndpoints();
app.MapInternalVideosEndpoints();
app.MapHealthEndpoints();

await app.RunAsync();
