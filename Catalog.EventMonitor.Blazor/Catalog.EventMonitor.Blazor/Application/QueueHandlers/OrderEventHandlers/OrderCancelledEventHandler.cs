using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class OrderCancelledEventHandler : IHandleMessages<OrderCancelledEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderCancelledEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderCancelledEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" отменены все процессы согласования!",
        };

        _eventStoreService.AddEvent(eventModel);
    }
}
