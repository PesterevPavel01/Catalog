using Calabonga.UnitOfWork;
using Catalog.Contracts.DomainEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.Handlers.DomainEventHandlers;

public sealed class CustomModuleChangedDomainEventHandler : INotificationHandler<CustomModuleChangedDomainEvent>
{
    private readonly IBus _bus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelegramService _telegramService;

    public CustomModuleChangedDomainEventHandler(IBus bus, IUnitOfWork unitOfWork,
        ITelegramService telegramService,
        IOptions<TelegramBotConfiguration> exceptionNotificationBot)
    {
        _unitOfWork = unitOfWork;
        _bus = bus;
        _telegramService = telegramService;
        _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
    }

    public async Task Handle(CustomModuleChangedDomainEvent customModuleChangedDomainEvent, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.GetRepository<Order>()
            .GetFirstOrDefaultAsync(
                predicate: x => x.Id == customModuleChangedDomainEvent.OrderId,
                include: Order.IncludeRequiredField()
            );

        if (order is null)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {typeof(OrderCancelledDomainEventHandler).Name}. Order not found!");
            return;
        }
        await _bus.Publish(new CustomModuleChangedEvent(order.ConvertToDto()));
    }
}