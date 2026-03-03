using Calabonga.UnitOfWork;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Commands;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.OrderService.Application.Messages.OrderEventMessages;
using Catalog.Redis;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

// TODO: Должен стать только классом для обновления кэша! и вызываться не через Rebus, а через MediatR

namespace Catalog.OrderService.Application.QueueHandlers;

public sealed class UpdateOrderCacheCommandHandler : IHandleMessages<UpdateOrderCacheCommand>
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

    public UpdateOrderCacheCommandHandler(
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

    public async Task Handle(UpdateOrderCacheCommand message)
    {
        var order = await _unitOfWork
            .GetRepository<Order>()
            .GetFirstOrDefaultAsync(
                trackingType: TrackingType.Tracking,
                include: Order.IncludeRequiredField(),
                predicate: x => x.Code == message.OrderCode);

        if (order is null)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.OrderCode}");
            return;
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

        var orderInvalidateResult = await _orderRedisService.InvalidateCacheAsync(Order.GenerateOrderCacheKey(_orderRedisService.GenerateCacheKey, message.OrderCode), default);

        if (!orderInvalidateResult.Ok)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {orderInvalidateResult.Error}");

            return;
        }

        if (order.OrderHistory.Last() is not null 
            && order.OrderHistory.Last().Type == (int)OrderEventType.Disabled 
            && order.OrderHistory.Last().Type != (int)OrderEventType.Deleted)
        {
            var orderQueryResult = await _orderQueryHandler.HandleAsync(message.OrderCode);

            if (!orderQueryResult.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {orderQueryResult.Error}");

                return;
            }
        }

    }
}