using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.ExchangeEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ExportEventHandlers;

public class RejectedEntitiesEventHandler : IHandleMessages<RejectedEntitiesEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBus _bus;
    private readonly ITelegramService _telegramService;

    public RejectedEntitiesEventHandler(IBus bus, IUnitOfWork unitOfWork, ITelegramService telegramService,
        IOptions<TelegramBotConfiguration> exceptionNotificationBot)
    {
        _unitOfWork = unitOfWork;
        _bus = bus;
        _telegramService = telegramService;
        _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
    }

    public async Task Handle(RejectedEntitiesEvent message)
    {
        if (message.Entities.Type != typeof(Order).Name)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Entity type not found!");
            return;
        }

        // TODO: Рефакторинг

        var orderRepository = _unitOfWork.GetRepository<Order>();

        var orders = await orderRepository
            .GetAllAsync(
                predicate: x => message.Entities.Codes.Contains(x.Code),
                include: Order.IncludeRequiredField(),
                trackingType: TrackingType.Tracking);

        if (!orders.Any())
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. order not found!");
            return;
        }

        foreach (var order in orders)
        {
            var rejectResult = order.RejectFromProduction();

            if (!rejectResult.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Error: {rejectResult.Error}!");
                continue;
            }

            orderRepository.Update(order);
        }

        var result = await _unitOfWork.SaveChangesAsync();

        if (_unitOfWork.Result.Exception is not null)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {_unitOfWork.Result.Exception.Message}!");
            return;
        }

        foreach (var order in orders)
            await _bus.Publish(new UpdateOrderCacheCommand(order.Code));
    }
}
