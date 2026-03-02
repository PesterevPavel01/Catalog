using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record WorkflowCreatedDomainEvent(Guid OrderId) : IDomainEvent;
