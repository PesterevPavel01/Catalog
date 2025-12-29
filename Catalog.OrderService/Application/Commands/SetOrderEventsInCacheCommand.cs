using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Interfaces;

namespace Catalog.OrderService.Application.Commands
{
    public sealed record SetOrderEventsInCacheCommand(string CacheKey, IEnumerable<OrderEventDto> Events) : IOrderQueueEvent;
}
