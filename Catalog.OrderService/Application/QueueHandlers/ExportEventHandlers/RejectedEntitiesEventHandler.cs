using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.ExchangeEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.OrderService.Application.QueueHandlers.ExportEventHandlers
{
    public class RejectedEntitiesEventHandler : IHandleMessages<RejectedEntitiesEvent>
    {
        private readonly IBus _bus;

        public RejectedEntitiesEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(RejectedEntitiesEvent message)
        {
            if (message.Entities.Type != typeof(Order).Name)
                return;

            foreach (var code in message.Entities.Codes)
                await _bus.Publish(new CreateOrderEventCommand(code, OrderEventType.ExternallyRejected, OrderEventTypeTitles.Produced));
        }
    }
}
