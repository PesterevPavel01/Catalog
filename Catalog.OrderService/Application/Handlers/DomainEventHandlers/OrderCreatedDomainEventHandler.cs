using Calabonga.UnitOfWork;
using Catalog.Contracts.DomainEvents;
using Catalog.Contracts.Events.OrderEvents;
using MediatR;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.Handlers.DomainEventHandlers;

public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IBus _bus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelegramService _telegramService;

    public OrderCreatedDomainEventHandler(IBus bus, IUnitOfWork unitOfWork,
        ITelegramService telegramService,
        IOptions<TelegramBotConfiguration> exceptionNotificationBot)
    {
        _unitOfWork = unitOfWork;
        _bus = bus;
        _telegramService = telegramService;
        _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
    }

    public async Task Handle(OrderCreatedDomainEvent orderCreatedDomainEvent, CancellationToken cancellationToken)
    {
        await _bus.Send(new OrderCreatedEvent(orderCreatedDomainEvent.Code));
    }
}
