using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class OrderDisabledEventHandler : IHandleMessages<OrderDisabledEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderDisabledEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderDisabledEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"ЗАКАЗ УДАЛЕН: заказ \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" был удален!"
        };

        _eventStoreService.AddEvent(eventModel);
    }
}