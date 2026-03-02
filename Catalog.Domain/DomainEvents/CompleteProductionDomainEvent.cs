using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record CompleteProductionDomainEvent(String OrderCode) : IDomainEvent;
