namespace Education.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity() => Id = Guid.NewGuid();
    protected Entity(Guid id) => Id = id;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj) =>
        obj is Entity other && GetType() == other.GetType() && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
