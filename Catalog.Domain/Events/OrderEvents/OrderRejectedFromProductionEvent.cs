using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.OrderEvents;

public sealed record OrderRejectedFromProductionEvent(OrderDto Order) : IOrderQueueEvent;
