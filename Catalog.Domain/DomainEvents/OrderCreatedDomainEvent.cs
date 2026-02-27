using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents
{
    public sealed record OrderCreatedDomainEvent(string Code) : IDomainEvent;
}
