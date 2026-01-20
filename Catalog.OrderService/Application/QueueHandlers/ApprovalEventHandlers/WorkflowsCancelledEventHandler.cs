using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.Approval;
using Catalog.Contracts.Resources;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowsCancelledEventHandler : IHandleMessages<WorkflowsCancelledEvent>
    {
        private readonly IBus _bus;

        public WorkflowsCancelledEventHandler(IBus bus, ITelegramService telegramService)
        {
            _bus = bus;
        }

        public async Task Handle(WorkflowsCancelledEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. order not found! Code: {message.Order.Code}");

            await _bus.Publish(new CreateOrderEventCommand(message.Order.Code, OrderEventTypes.ApprovalCompleted, OrderEventTypeTitles.ApprovalCompleted));

            return;
        }
    }
}
