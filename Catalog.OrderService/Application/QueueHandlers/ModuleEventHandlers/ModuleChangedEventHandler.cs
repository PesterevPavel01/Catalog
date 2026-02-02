using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.OrderService.Application.QueueHandlers.ModuleEventHandlers
{
    public class ModuleChangedEventHandler : IHandleMessages<ModuleChangedEvent>
    {
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;

        public ModuleChangedEventHandler(IUnitOfWork unitOfWork, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _bus = bus;
        }

        public async Task Handle(ModuleChangedEvent message)
        {
            var orders = await _unitOfWork.GetRepository<Order>()
                .GetAllAsync(
                    predicate: x => x.OrderItems.Any(item => item.Module.Id == message.ModuleId),
                    trackingType: TrackingType.NoTracking
                );

            foreach (var order in orders) {

                await _bus.Publish(new CreateOrderEventCommand(order.Code, OrderEventTypes.Changed, OrderEventTypeTitles.Changed));

            }

            return;
        }
    }
}
