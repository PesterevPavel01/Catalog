using Catalog.Contracts.ApplicationEvents;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.OrderService.Application.QueueHandlers.ExportEventHandlers
{
    public class EntitiesExportedEventHandler : IHandleMessages<EntitiesExportedEvent>
    {
        private readonly IBus _bus;

        public EntitiesExportedEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(EntitiesExportedEvent message)
        {
            if (message.Entities.Type != typeof(Order).Name)
                return;

            foreach(var code in message.Entities.Codes)
                await _bus.Publish(new CreateOrderEventCommand(code, OrderEventTypes.Exported, OrderEventTypeTitles.Exported));
        }
    }
}
