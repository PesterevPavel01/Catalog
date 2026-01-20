using Catalog.Contracts.Events.Approval;
using Catalog.Contracts.Events.ApprovalEvents;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.ApprovalService.Application.QueueHandlers
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
                throw new ArgumentException($"{"ApprovalService".ToUpper()} Event {message.GetType().Name}. Order not found!");

            //only for IsCustom orders
            if (!message.Order.IsCustom)
            {
                await _bus.Publish(new WorkflowsCancelledEvent(message.Order));
            }
            return;
        }
    }
}