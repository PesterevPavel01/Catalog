using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record OrderCompletedDomainEvent(Guid OrderId) : IDomainEvent;
