using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents
{
    public sealed record OrderChangedDomainEvent(string Code) : IDomainEvent;
}
