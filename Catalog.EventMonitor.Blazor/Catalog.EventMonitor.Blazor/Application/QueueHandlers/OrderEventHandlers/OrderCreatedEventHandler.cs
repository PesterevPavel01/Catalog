using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Rebus.Handlers;

namespace Catalog.EventMonitor.Application.QueueHandlers.ApprovalEventHandlers
{
    public class OrderCreatedEventHandler : IHandleMessages<OrderCreatedEvent>
    {
        private readonly IEventStoreService  _eventStoreService;

        public OrderCreatedEventHandler(IEventStoreService eventStoreService)
        {
            _eventStoreService = eventStoreService;
        }

        public async Task Handle(OrderCreatedEvent message)
        {
            var eventModel = new OrderEventModel
            {
                OrderCode = message.OrderCode,
                EventType = "OrderCreated",
                Details = $"СОЗДАНИЕ ЗАКАЗА: создан новый заказ с кодом: {message.OrderCode}!"
            };

            _eventStoreService.AddEvent(eventModel);
        }
    }
}