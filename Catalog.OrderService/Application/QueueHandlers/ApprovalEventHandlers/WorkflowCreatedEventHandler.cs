using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Resources;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowCreatedEventHandler : IHandleMessages<WorkflowCreatedEvent>
    {
        private readonly IBus _bus;

        public WorkflowCreatedEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(WorkflowCreatedEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found!");

            await _bus.Publish(new CreateOrderEventCommand(message.Order.Code, OrderEventTypes.CreateApprovalWorkflow, OrderEventTypeTitles.CreateApprovalWorkflow));

            return;
        }
    }
}