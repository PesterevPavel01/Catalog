using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ApprovalEventHandlers;

public class WorkflowCompletedEventHandler : IHandleMessages<WorkflowCompletedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBus _bus;
    private readonly ITelegramService _telegramService;

    public WorkflowCompletedEventHandler(IBus bus, IUnitOfWork unitOfWork, ITelegramService telegramService,
        IOptions<TelegramBotConfiguration> exceptionNotificationBot)
    {
        _unitOfWork = unitOfWork;
        _bus = bus;
        _telegramService = telegramService;
        _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
    }

    public async Task Handle(WorkflowCompletedEvent message)
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

        var approvalCompletedResult = order.ApprovalComplete();

        if (!approvalCompletedResult.Ok)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {approvalCompletedResult.Error}! Code: {message.Order.Code}");
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