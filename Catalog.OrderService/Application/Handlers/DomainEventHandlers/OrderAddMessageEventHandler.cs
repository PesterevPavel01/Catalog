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

public class AddMessageDomainEventHandler : INotificationHandler<AddMessageDomainEvent>
{
    private readonly IBus _bus;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelegramService _telegramService;

    public AddMessageDomainEventHandler(IBus bus, IUnitOfWork unitOfWork,
        ITelegramService telegramService,
        IOptions<TelegramBotConfiguration> exceptionNotificationBot)
    {
        _bus = bus;
    }

    public async Task Handle(AddMessageDomainEvent addMessageDomainEvent, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.GetRepository<Order>()
            .GetFirstOrDefaultAsync(
                predicate: x => x.Code == addMessageDomainEvent.OrderCode,
                include: Order.IncludeRequiredField()
            );

        if (order is null)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {typeof(OrderCancelledDomainEventHandler).Name}. Order not found!");
            return;
        }
        await _bus.Publish(new OrderAddMessageEvent(order.ConvertToDto()));
    }
}
