using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.DomainEvents;

public sealed record RemoveOrderItemDomainEvent(Guid OrderId) : IDomainEvent;
