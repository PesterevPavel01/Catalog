using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Resources;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.OrderService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class CustomWorkflowChangedEventHandler : IHandleMessages<CustomWorkflowChangedEvent>
    {
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;

        public CustomWorkflowChangedEventHandler(IBus bus, IUnitOfWork unitOfWork)
        {
            _bus = bus;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CustomWorkflowChangedEvent message)
        {
            var workflow = await _unitOfWork
                .GetRepository<ApprovalWorkflow>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.NoTracking,
                    include: query => query
                        .Include(x => x.OrderItem)
                            .ThenInclude(x => x.Order),
                    predicate: x => x.Id == message.WorkflowId);

            if (workflow is null)
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. workflow not found! ID: {message.WorkflowId}");

            await _bus.Publish(new CreateOrderEventCommand(workflow.OrderItem.Order.Code, OrderEventTypes.CustomModuleModified, OrderEventTypeTitles.CustomModuleModified));

            return;
        }
    }
}