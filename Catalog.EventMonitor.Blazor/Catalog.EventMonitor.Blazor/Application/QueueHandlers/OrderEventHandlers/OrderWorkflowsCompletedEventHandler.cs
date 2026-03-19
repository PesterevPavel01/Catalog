using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class OrderWorkflowsCompletedEventHandler : IHandleMessages<OrderWorkflowsCompletedEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public OrderWorkflowsCompletedEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(OrderWorkflowsCompletedEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" завершен процесс согласования модуля!"
        };

        _eventStoreService.AddEvent(eventModel);
    }
}
