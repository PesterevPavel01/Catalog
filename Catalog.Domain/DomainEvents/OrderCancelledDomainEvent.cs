using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record OrderCancelledDomainEvent(Guid OrderId) : IDomainEvent;
