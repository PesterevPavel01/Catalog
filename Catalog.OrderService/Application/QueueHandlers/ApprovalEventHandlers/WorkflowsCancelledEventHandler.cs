using Catalog.Contracts.Commands;
using Catalog.Contracts.Events.Approval;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowsCancelledEventHandler : IHandleMessages<WorkflowsCancelledEvent>
    {
        private readonly IBus _bus;
        private readonly ILogger<WorkflowsCancelledEventHandler> _logger;

        public WorkflowsCancelledEventHandler(IBus bus, ILogger<WorkflowsCancelledEventHandler> logger, ITelegramService telegramService)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task Handle(WorkflowsCancelledEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. order not found! Code: {message.Order.Code}");

            await _bus.Publish(new CreateOrderEventCommand(message.Order.Code, "Завершен процесс согласования."));

            return;
        }
    }
}
