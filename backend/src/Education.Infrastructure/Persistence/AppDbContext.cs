using Education.Domain.Common;
using Education.Domain.Entities.Auth;
using Education.Domain.Entities.Courses;
using Education.Domain.Entities.Videos;
using Education.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Education.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly IPublisher _publisher;

    public AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher)
        : base(options) => _publisher = publisher;

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Video> Videos => Set<Video>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result;
        try
        {
            result = await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // O Domain/Application não conhecem EF: traduz para a exceção do domínio.
            throw new ConcurrencyException("O registro foi alterado por outro processo.", ex);
        }

        await PublishDomainEventsAsync(cancellationToken);
        return result;
    }

    /// <summary>Eventos só são publicados depois do commit: quem reage vê o dado já gravado.</summary>
    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);
    }
}
