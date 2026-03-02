using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Contracts.Entities.Base;

public abstract class AggregateRoot : SimpleEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TitleValue title, string code, Guid id) : base(title, code, id){}

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        return [.. _domainEvents];
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
