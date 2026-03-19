using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class OrderCompletedEventHandler : IHandleMessages<OrderCompletedEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderCompletedEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderCompletedEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"ЗАКАЗ ЗАВЕРШЕН: заказ \"{message.Order.Title}\" пользователя: \"{message.Order.User}\"."
        };

        _eventStoreService.AddEvent(eventModel);
    }
}