using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Resources;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class OrderApprovalWorkflowsRemoveEventHandler : IHandleMessages<OrderApprovalWorkflowsRemoveEvent>
    {
        private readonly IBus _bus;

        public OrderApprovalWorkflowsRemoveEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(OrderApprovalWorkflowsRemoveEvent message)
        {
            await _bus.Publish(new CreateOrderEventCommand(message.Order.Code, OrderEventTypes.Cancelled, OrderEventTypeTitles.Cancelled));

            return;
        }
    }
}
