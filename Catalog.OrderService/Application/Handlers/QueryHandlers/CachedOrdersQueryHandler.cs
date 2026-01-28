using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Commands;
using Catalog.Redis;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Handlers.QueryHandlers
{
    public class CachedOrdersQueryHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RedisServiceFactory _redisServiceFactory;
        private readonly IBus _bus;

        public CachedOrdersQueryHandler(IUnitOfWork unitOfWork, RedisServiceFactory redisServiceFactory, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _redisServiceFactory = redisServiceFactory;
            _bus = bus;
        }

        public async Task<Operation<List<OrderDto>, string>> HandleAsync(string cacheKeyType, string? userLogin = null, CancellationToken cancellationToken = default)
        {
            IEnumerable<OrderDto> orders = [];

            var redisService = _redisServiceFactory.GetService<OrderDto>();

            var cacheKey = redisService.GenerateCacheKey(("type", cacheKeyType), ("days", Order.CacheDays));

            var cachedOrders = await redisService.GetFromCacheAsync(cacheKey, cancellationToken);

            if (cachedOrders.Ok)
                return cachedOrders.Result.ToList();
            else
            {
                orders = await _unitOfWork
                .GetRepository<Order>()
                .GetAllAsync(
                    predicate: x =>
                        (userLogin == null || x.ApplicationUser.UserName == userLogin)
                        && x.Enabled
                        &&  x.CreatedAt > DateTime.Now.AddDays(-1 * Order.CacheDays),
                        include: Order.IncludeRequiredField(),
                    selector: x => x.ConvertToDto(),
                    trackingType: TrackingType.NoTracking);

                await _bus.Send(new CacheOrdersCommand(cacheKey, orders));

                return orders.ToList();
            }
        }
    }
}
