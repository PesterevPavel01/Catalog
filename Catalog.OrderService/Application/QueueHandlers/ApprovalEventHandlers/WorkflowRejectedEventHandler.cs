using Catalog.Contracts.Commands;
using Catalog.Contracts.Events.ApprovalEvents;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public sealed class WorkflowRejectedEventHandler : IHandleMessages<WorkflowRejectedEvent>
    {
        private readonly IBus _bus;

        public WorkflowRejectedEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(WorkflowRejectedEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found!");

            //only for IsCustom orders
            if (message.Order.Modules.FirstOrDefault(x => x.Module.IsCustom) is not null)
            {
                await _bus.Publish(new CreateOrderEventCommand(message.Order.Code, "Не пройден процесс согласования."));
            }
            return;
        }
    }
}
