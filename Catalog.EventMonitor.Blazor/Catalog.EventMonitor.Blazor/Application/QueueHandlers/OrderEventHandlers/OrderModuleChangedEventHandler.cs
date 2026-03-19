using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class OrderModuleChangedEventHandler : IHandleMessages<OrderModuleChangedEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderModuleChangedEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderModuleChangedEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"ИЗМЕНЕНИЕ ЗАКАЗА: у заказа \"{message.Order.Code}\" пользователя: \"{message.Order.User}\" произошли изменения модуля!"
        };

        _eventStoreService.AddEvent(eventModel);
    }
}