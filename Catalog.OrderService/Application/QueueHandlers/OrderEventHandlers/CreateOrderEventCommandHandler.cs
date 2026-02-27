using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Enum;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Commands;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.OrderService.Application.Messages.OrderEventMessages;
using Catalog.OrderService.Application.Messages.OrderMessages;
using Catalog.Redis;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.OrderEventHandlers
{
    public sealed class CreateOrderEventCommandHandler : IHandleMessages<CreateOrderEventCommand>
    {
        private readonly IMediator _mediator;
        private readonly ITelegramService _telegramService;
        private readonly CachedOrdersQueryHandler _cachedOrdersQueryHandler;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;
        private readonly RedisService<OrderEventDto> _orderEventRedisService;
        private readonly RedisService<OrderDto> _orderRedisService;
        private readonly OrderQueryHandler _orderQueryHandler;
        private readonly string? _completionTriggerEventType;

        public CreateOrderEventCommandHandler(
            OrderQueryHandler orderQueryHandler,
            CachedOrdersQueryHandler ordersQueryHandler,
            IMediator mediator,
            RedisServiceFactory redisServiceFactory,
            ITelegramService telegramService,
            IUnitOfWork unitOfWork, IBus bus,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot,
            IOptions<OrderConfiguration> orderConfiguration)
        {
            _mediator = mediator;
            _orderQueryHandler = orderQueryHandler;
            _unitOfWork = unitOfWork;
            _bus = bus;
            _orderEventRedisService = redisServiceFactory.GetService<OrderEventDto>();
            _orderRedisService = redisServiceFactory.GetService<OrderDto>();
            _cachedOrdersQueryHandler = ordersQueryHandler;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
            _completionTriggerEventType = orderConfiguration.Value.CompletionTriggerEventType;
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
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.OrderCode}");

                return;
            }

            if (message.Type != OrderEventType.Created)
            {
                var orderEvent = OrderEvent.Create(message.Note, message.Type, null);

                if (!orderEvent.Ok)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {orderEvent.Error} OrderTitle: {order.Title}");

                    return;
                }

                order.AddOrderEvent(orderEvent.Result);

                await _unitOfWork.GetRepository<OrderEvent>().InsertAsync(orderEvent.Result);

                var result = await _unitOfWork.SaveChangesAsync();

                if (_unitOfWork.Result.Exception is not null)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {_unitOfWork.Result.Exception.Message} OrderTitle: {order.Title}");

                    return;
                }

            }

            /*CACHING*/
            /*ORDER EVENTS*/

            var queryResult = await _mediator.Send(new GetOrderEvents.Request(0, OrderEvent.CacheEntriesCount), default);

            if (!queryResult.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {queryResult.Error} OrderTitle: {order.Title}");

                return;
            }

            await _bus.Send(new CacheOrderEventsCommand(OrderEvent.GenerateConstructorCommonCacheKey(_orderEventRedisService.GenerateCacheKey), queryResult.Result.Items));

            if (order.ApplicationUser.Roles.Any(x => x.Code == "customer")) {

                queryResult = await _mediator.Send(new GetUserOrderEvents.Request(order.ApplicationUser.UserName, 0, OrderEvent.CacheEntriesCount), default);

                if (!queryResult.Ok)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {queryResult.Error} OrderTitle: {order.Title}");

                    return;
                }

                await _bus.Send(new CacheOrderEventsCommand(OrderEvent.GenerateUserCommonCacheKey(_orderEventRedisService.GenerateCacheKey, order.ApplicationUser.UserName), queryResult.Result.Items));
            }

            /*ORDERS*/

            if (message.Type.IsCaching())
            {
                var customerOrdersCacheKey = Order.GenerateUserCommonCacheKey(_orderRedisService.GenerateCacheKey, order.ApplicationUser.UserName);

                var customerInvalidateResult = await _orderRedisService.InvalidateCacheAsync(customerOrdersCacheKey, default);

                if (!customerInvalidateResult.Ok)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {customerInvalidateResult.Error}");

                    return;
                }
                
                var customerOrdersResult = await _cachedOrdersQueryHandler.HandleAsync(cacheKey: customerOrdersCacheKey, userLogin: order.ApplicationUser.UserName, default);

                if (!customerOrdersResult.Ok)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {customerOrdersResult.Error}");

                    return;
                }

                var constructorCacheKey = Order.GenerateConstructorCommonCacheKey(_orderRedisService.GenerateCacheKey);

                var constructorInvalidateResult = await _orderRedisService.InvalidateCacheAsync(constructorCacheKey, default);

                if (!constructorInvalidateResult.Ok)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {constructorInvalidateResult.Error}");

                    return;
                }

                var constructorOrdersResult = await _cachedOrdersQueryHandler.HandleAsync(cacheKey: constructorCacheKey, default);

                if (!constructorOrdersResult.Ok)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {constructorOrdersResult.Error}");

                    return;
                }

                var orderCacheKey = _orderRedisService.GenerateCacheKey;

                var orderInvalidateResult = await _orderRedisService.InvalidateCacheAsync(Order.GenerateOrderCacheKey(_orderRedisService.GenerateCacheKey, message.OrderCode), default);

                if (!orderInvalidateResult.Ok)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {orderInvalidateResult.Error}");

                    return;
                }

                if (message.Type != OrderEventType.Disabled && message.Type != OrderEventType.Deleted)
                {
                    var orderQueryResult = await _orderQueryHandler.HandleAsync(message.OrderCode);

                    if (!orderQueryResult.Ok)
                    {
                        await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {orderQueryResult.Error}");

                        return;
                    }
                }

                /*Auto Complete Order Command*/

                if (_completionTriggerEventType is not null)
                {
                    if (Enum.TryParse<OrderEventType>(_completionTriggerEventType, out var completionTriggerEventType))
                    {
                        if (message.Type == completionTriggerEventType)
                        {
                            var completeResult = await _mediator.Send(new CompleteOrder.Request([message.OrderCode]), default);
                            
                            if (!completeResult.Ok)
                                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {completeResult.Error} OrderTitle: {order.Title.Value}");
                        }
                    }
                    else
                    {
                        await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Неверное название типа события \"CompletionTriggerEventType\" в конфигурации! OrderTitle: {order.Title}");
                    }
                }
            }

        }
    }
}