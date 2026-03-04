using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record ModuleChangedDomainEvent(Guid OrderId) : IDomainEvent;
