using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Events.ApprovalEvents;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowCreatedEventHandler : IHandleMessages<WorkflowCreatedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;

        public WorkflowCreatedEventHandler(IUnitOfWork unitOfWork, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _bus = bus;
        }

        public async Task Handle(WorkflowCreatedEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found!");

            //only for IsCustom orders
            if (message.Order.Modules.FirstOrDefault(x => x.Module.IsCustom) is not null)
            {
                await _bus.Publish(new CreateOrderEventCommand(message.Order.Code, "Запущен процесс согласования."));
            }
            return;
        }
    }
}