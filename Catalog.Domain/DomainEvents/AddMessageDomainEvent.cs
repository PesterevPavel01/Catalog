using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record AddMessageDomainEvent(String OrderCode) : IDomainEvent;
