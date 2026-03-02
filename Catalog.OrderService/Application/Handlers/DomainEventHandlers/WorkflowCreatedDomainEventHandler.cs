using Calabonga.UnitOfWork;
using Catalog.Contracts.DomainEvents;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.Handlers.DomainEventHandlers;

public class WorkflowCreatedDomainEventHandler : INotificationHandler<WorkflowCreatedDomainEvent>
{
    private readonly IBus _bus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelegramService _telegramService;

    public WorkflowCreatedDomainEventHandler(IBus bus, IUnitOfWork unitOfWork,
        ITelegramService telegramService,
        IOptions<TelegramBotConfiguration> exceptionNotificationBot)
    {
        _unitOfWork = unitOfWork;
        _bus = bus;
        _telegramService = telegramService;
        _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
    }

    public async Task Handle(WorkflowCreatedDomainEvent workflowCreatedDomainEvent, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.GetRepository<Order>()
            .GetFirstOrDefaultAsync(
                predicate: x => x.Id == workflowCreatedDomainEvent.OrderId,
                include: Order.IncludeRequiredField()
            );

        if (order is null)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {typeof(OrderCancelledDomainEventHandler).Name}. Order not found!");
            return;
        }
        await _bus.Publish(new OrderWorkflowCreatedEvent(order.ConvertToDto()));
    }
}