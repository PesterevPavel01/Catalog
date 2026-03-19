using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class OrderExportedEventHandler : IHandleMessages<OrderExportedEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderExportedEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderExportedEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"ПЕРЕДАН В ПРОИЗВОДСТВО: заказ \"{message.Order.Title}\" пользователя: \"{message.Order.User}\"."
        };

        _eventStoreService.AddEvent(eventModel);
    }
}