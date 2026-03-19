using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.OrderEventHandlers;

public class MarkOrdersAsProducedEventHandler : IHandleMessages<MarkOrdersAsProducedEvent>
{
    private readonly IEventStoreService _eventStoreService;

    public MarkOrdersAsProducedEventHandler(IEventStoreService eventStoreService)
    {
        _eventStoreService = eventStoreService;
    }

    public async Task Handle(MarkOrdersAsProducedEvent message)
    {
        var eventModel = new OrderEventModel
        {
            Title = message.Order.Title,
            Details = $"ПРОИЗВОДСТВО ЗАВЕРШЕНО: заказ \"{message.Order.Title}\" пользователя: \"{message.Order.User}\".",
        };

        _eventStoreService.AddEvent(eventModel);
    }
}