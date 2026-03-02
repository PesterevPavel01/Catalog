using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record AddOrderItemDomainEvent(Guid OrderId) : IDomainEvent;
