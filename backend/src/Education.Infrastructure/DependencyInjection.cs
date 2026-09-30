using Education.Application.Courses.Queries;
using Education.Application.Lessons.Queries;
using Education.Domain.Interfaces;
using Education.Domain.Interfaces.Auth;
using Education.Domain.Interfaces.Courses;
using Education.Domain.Interfaces.Videos;
using Education.Infrastructure.Authentication;
using Education.Infrastructure.Messaging;
using Education.Infrastructure.Persistence;
using Education.Infrastructure.Persistence.Repositories;
using Education.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Education.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Escrita (agregados)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();

        // Leitura (queries projetadas em DTOs)
        services.AddScoped<ICourseReadRepository, CourseReadRepository>();
        services.AddScoped<ILessonReadRepository, LessonReadRepository>();

        // Autenticação: emissão de tokens e hash de senha (a validação do JWT fica no CrossCutting)
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // Arquivos de vídeo (Docker Volume no MVP) e fila para o Worker
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IMessageBus, RabbitMqMessageBus>();

        return services;
    }
}
