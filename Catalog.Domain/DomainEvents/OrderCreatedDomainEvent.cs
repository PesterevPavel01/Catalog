using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record OrderCreatedDomainEvent(String Code) : IDomainEvent;
