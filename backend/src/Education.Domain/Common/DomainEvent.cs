namespace Education.Domain.Common;

public abstract record DomainEvent(Guid EventId, DateTime OccurredOn) : IDomainEvent
{
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow) { }
}
