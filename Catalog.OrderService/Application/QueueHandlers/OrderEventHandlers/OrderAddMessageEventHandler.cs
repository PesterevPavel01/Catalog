using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.NotificationService.Application.QueueHandlers.OrderEventHandlers
{
    public class OrderAddMessageEventHandler : IHandleMessages<OrderAddMessageEvent>
    {
        private readonly IBus _bus; 

        public OrderAddMessageEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(OrderAddMessageEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found!");

            await _bus.Publish(new UpdateOrderCacheCommand(message.Order.Code));
        }
    }
}