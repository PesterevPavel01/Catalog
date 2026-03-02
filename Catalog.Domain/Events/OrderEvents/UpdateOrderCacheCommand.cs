using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.OrderEvents
{
    public record UpdateOrderCacheCommand(string OrderCode) : IOrderQueueEvent;
}
