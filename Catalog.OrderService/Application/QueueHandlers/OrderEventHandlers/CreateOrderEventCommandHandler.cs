using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Enum;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Commands;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.Redis;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.OrderEventHandlers
{
    public sealed class CreateOrderEventCommandHandler : IHandleMessages<CreateOrderEventCommand>
    {
        private readonly ConstructorOrderEventQueriesHandler _constructorCommandHandler;
        private readonly ApplicationUserOrderEventQueriesHandler _customerCommandHandler;
        private readonly CachedOrdersQueryHandler _cachedOrdersQueryHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;
        private readonly RedisService<OrderEventDto> _orderEventRedisService;
        private readonly RedisService<OrderDto> _orderRedisService;

        public CreateOrderEventCommandHandler(
            CachedOrdersQueryHandler ordersQueryHandler,
            ConstructorOrderEventQueriesHandler constructorCommandHandler, ApplicationUserOrderEventQueriesHandler customerCommandHandler,
            RedisServiceFactory redisServiceFactory, ITelegramService telegramService, IUnitOfWork unitOfWork, IBus bus)
        {
            _constructorCommandHandler = constructorCommandHandler;
            _customerCommandHandler = customerCommandHandler;
            _unitOfWork = unitOfWork;
            _bus = bus;
            _orderEventRedisService = redisServiceFactory.GetService<OrderEventDto>();
            _orderRedisService = redisServiceFactory.GetService<OrderDto>();
            _cachedOrdersQueryHandler = ordersQueryHandler;
        }

        public async Task Handle(CreateOrderEventCommand message)
        {
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.Tracking,
                    include: query => query
                        .Include(x => x.ApplicationUser)
                        .ThenInclude(x=> x.Roles),
                    predicate: x => x.Code == message.OrderCode);

            if (order is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.OrderCode}");

            var orderEvent = OrderEvent.Create(message.Note, message.Type, null);

            if (!orderEvent.Ok)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {orderEvent.Error} OrderTitle: {order.Title}");

            order.AddOrderEvent(orderEvent.Result);

            await _unitOfWork.GetRepository<OrderEvent>().InsertAsync(orderEvent.Result);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {_unitOfWork.Result.Exception.Message} OrderTitle: {order.Title}");
            }

            var queryResult = await _constructorCommandHandler.HandleAsync(default);

            if (!queryResult.Ok)
                throw new ArgumentException(queryResult.Error);

            await _bus.Send(new CacheOrderEventsCommand(_orderEventRedisService.GenerateCacheKey(("type", "constructor")), queryResult.Result.Items));

            if (order.ApplicationUser.Roles.Any(x => x.Code == "customer")) {

                queryResult = await _customerCommandHandler.HandleAsync(order.ApplicationUser.UserName);
                    if (!queryResult.Ok)
                        throw new ArgumentException(queryResult.Error);

                await _bus.Send(new CacheOrderEventsCommand(_orderEventRedisService.GenerateCacheKey(("type", order.ApplicationUser.UserName)), queryResult.Result.Items));
            }

            var eventType = (OrderEventTypes)orderEvent.Result.Type;

            if (eventType.IsCaching())
            {
                var customerCacheKey = _orderRedisService.GenerateCacheKey(("type", order.ApplicationUser.UserName), ("days", Order.CacheDays));

                var customerInvalidateResult = await _orderRedisService.InvalidateCacheAsync(customerCacheKey, default);

                var customerOrdersResult = await _cachedOrdersQueryHandler.HandleAsync(cacheKeyType:order.ApplicationUser.UserName, userLogin: order.ApplicationUser.UserName, default);

                var constructorCacheKey = _orderRedisService.GenerateCacheKey(("type", "constructor"), ("days", Order.CacheDays));

                var constructorInvalidateResult = await _orderRedisService.InvalidateCacheAsync(constructorCacheKey, default);

                var constructorOrdersResult = await _cachedOrdersQueryHandler.HandleAsync(cacheKeyType: "constructor", default);
            }

        }
    }
}