using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record CustomModuleChangedDomainEvent(Guid OrderId) : IDomainEvent;
