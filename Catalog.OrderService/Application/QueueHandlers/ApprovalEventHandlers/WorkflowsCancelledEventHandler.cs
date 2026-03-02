using Calabonga.UnitOfWork;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.Approval;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers;

public class WorkflowsCancelledEventHandler : IHandleMessages<WorkflowsCancelledEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBus _bus;
    private readonly ITelegramService _telegramService;

    public WorkflowsCancelledEventHandler(IBus bus, IUnitOfWork unitOfWork, ITelegramService telegramService,
        IOptions<TelegramBotConfiguration> exceptionNotificationBot)
    {
        _unitOfWork = unitOfWork;
        _bus = bus;
        _telegramService = telegramService;
        _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
    }

    public async Task Handle(WorkflowsCancelledEvent message)
    {
        if (message.Order is null) 
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. order not found! Code: {message.Order.Code}");
            return;
        }

        // TODO: Рефакторинг

        var orderRepository = _unitOfWork.GetRepository<Order>();

        var order = await orderRepository
            .GetFirstOrDefaultAsync(
                predicate: x => x.Code == message.Order.Code,
                include: Order.IncludeRequiredField(),
                trackingType: TrackingType.Tracking);

        if (order is null)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. order not found! Code: {message.Order.Code}");
            return;
        }

        var cancelResult = order.Cancel();

        if (!cancelResult.Ok)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {cancelResult.Error}! Code: {message.Order.Code}");
            return;
        }

        orderRepository.Update(order);

        var result = await _unitOfWork.SaveChangesAsync();

        if (_unitOfWork.Result.Exception is not null)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {_unitOfWork.Result.Exception.Message}! Code: {message.Order.Code}");
            return;
        }

        await _bus.Publish(new UpdateOrderCacheCommand(message.Order.Code));

        return;
    }
}
