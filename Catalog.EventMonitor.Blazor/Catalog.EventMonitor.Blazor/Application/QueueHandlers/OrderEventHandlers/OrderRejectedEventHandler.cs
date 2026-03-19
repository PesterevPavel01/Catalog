using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public sealed class OrderRejectedEventHandler : IHandleMessages<OrderRejectedEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderRejectedEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderRejectedEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" не пройден процесс согласования модуля!"
        };

        _eventStoreService.AddEvent(eventModel);
    }
}
