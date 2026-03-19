using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public sealed class OrderRejectedFromProductionEventHandler : IHandleMessages<OrderRejectedFromProductionEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderRejectedFromProductionEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderRejectedFromProductionEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"ЗАКАЗ НЕ ПРИНЯТ В ПРОИЗВОДСТВО: заказ: \"{message.Order.Title}\" пользователя: \"{message.Order.User}\"!"
        };

        _eventStoreService.AddEvent(eventModel);
    }
}
