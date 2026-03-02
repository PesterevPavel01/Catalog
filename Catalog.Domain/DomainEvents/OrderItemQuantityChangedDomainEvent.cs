using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record OrderItemQuantityChangedDomainEvent(String OrderCode) : IDomainEvent;
