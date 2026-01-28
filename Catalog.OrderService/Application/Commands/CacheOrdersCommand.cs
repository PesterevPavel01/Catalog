using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;

namespace Catalog.OrderService.Application.Commands
{
    public sealed record CacheOrdersCommand(string CacheKey, IEnumerable<OrderDto> Orders) : IOrderQueueEvent;
}
