using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.OrderEvents
{
    public sealed record OrderExportedEvent(OrderDto Order) : IOrderQueueEvent;
}
