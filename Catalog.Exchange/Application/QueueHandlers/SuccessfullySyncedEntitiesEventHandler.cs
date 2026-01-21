using Catalog.Contracts.Commands;
using Catalog.ExchangeService.Application.Events;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.ExchangeService.Application.QueueHandlers
{
    public class SuccessfullySyncedEntitiesEventHandler : IHandleMessages<SuccessfullySyncedEntitiesEvent>
    {
        private readonly IBus _bus;

        public SuccessfullySyncedEntitiesEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(SuccessfullySyncedEntitiesEvent message)
        {
            foreach(var order in message.Models)
                await _bus.Publish(new UpdateOrderCodeCommand(order.SourceCode, order.ExternalCode));
        }
    }
}