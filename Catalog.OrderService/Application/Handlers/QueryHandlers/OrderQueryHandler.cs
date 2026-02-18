using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Commands;
using Catalog.Redis;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Handlers.QueryHandlers
{
    public class OrderQueryHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RedisService<OrderDto> _orderRedisService;
        private readonly IBus _bus;

        public OrderQueryHandler(IUnitOfWork unitOfWork, RedisServiceFactory redisServiceFactory, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _orderRedisService = redisServiceFactory.GetService<OrderDto>();
            _bus = bus;
        }

        public async Task<Operation<OrderDto, string>> HandleAsync(string code, bool cache = false, CancellationToken cancellationToken = default)
        {
            OrderDto? order = null;

            var orderCacheKey = Order.GenerateOrderCacheKey(_orderRedisService.GenerateCacheKey, code);

            if (string.IsNullOrWhiteSpace(code))
                return Operation.Error("Parameter OrderCode not found!");

            if (cache)
            { 
                var cachedOrders = await _orderRedisService.GetFromCacheAsync(orderCacheKey, cancellationToken);

                if (cachedOrders.Ok)
                    order = cachedOrders.Result.FirstOrDefault(x => x.Code == code);
            }

            if (order is null)
            {
                order = await _unitOfWork
                    .GetRepository<Order>()
                    .GetFirstOrDefaultAsync(
                        predicate: x =>
                            x.Code == code
                            && x.Enabled,
                        include: Order.IncludeRequiredField(),
                        trackingType: TrackingType.NoTracking,
                        selector: x => x.ConvertToDto());

                if (order is null)
                    return Operation.Error("Order not found!");

                if (cache)
                    await _bus.Send(new CacheOrdersCommand(orderCacheKey, [order]));
            }

            return order;
        }
    }
}