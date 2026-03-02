using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record OrderRejectFromProductionDomainEvent(Guid OrderId) : IDomainEvent; 
