using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class OrderWorkflowCreatedEventHandler : IHandleMessages<OrderWorkflowCreatedEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderWorkflowCreatedEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderWorkflowCreatedEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" запущен новый процесс согласования!"
        };

        _eventStoreService.AddEvent(eventModel);
    }
}