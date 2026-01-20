using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.OrderService.Application.QueueHandlers.OrderEventHandlers
{
    public class OrderChangedEventHandler : IHandleMessages<OrderChangedEvent>
    {
        private readonly IBus _bus;

        public OrderChangedEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(OrderChangedEvent message)
        {
            if (String.IsNullOrWhiteSpace(message.OrderCode))
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order code not found!");

            await _bus.Publish(new CreateOrderEventCommand(message.OrderCode, OrderEventTypes.Changed, OrderEventTypeTitles.Changed));
        }
    }
}