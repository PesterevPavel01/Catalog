using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class OrderAddMessageEventHandler : IHandleMessages<OrderAddMessageEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderAddMessageEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderAddMessageEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"НОВЫЙ КОММЕНТАРИЙ: к заказу \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" добавлен новый комментарий!",
        };

        _eventStoreService.AddEvent(eventModel);
    }
}