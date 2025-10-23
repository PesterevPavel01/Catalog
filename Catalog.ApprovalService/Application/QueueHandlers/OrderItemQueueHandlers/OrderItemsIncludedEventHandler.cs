using Catalog.ApprovalService.Application.Services;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Events.OrderEvents;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.ApprovalService.Application.QueueHandlers.OrderItemQueueHandlers
{
    public class OrderItemsIncludedEventHandler : IHandleMessages<OrderItemsIncludedEvent>
    {
        private readonly ILogger<OrderItemsIncludedEventHandler> _logger;

        private readonly OrderItemApprovalInitiatorService _processor;

        private readonly IBus _bus;

        public OrderItemsIncludedEventHandler(IBus bus, OrderItemApprovalInitiatorService processor, ILogger<OrderItemsIncludedEventHandler> logger)
        {
            _processor = processor;
            _logger = logger;
            _bus = bus;
        }

        public async Task Handle(OrderItemsIncludedEvent message)
        {
            var result = await _processor.InitializeAsync(message.models, new CancellationToken());

            if (!result.Ok)
                return;

            var createdOrderItems = result.Result.OrderItems.Where(x => message.models.Select(m => m.ModuleCode).Contains(x.Module.Code));

            if (createdOrderItems.FirstOrDefault(x => x.Module.IsCustom) is not null)
            {
                await _bus.Publish(new WorkflowCreatedEvent(result.Result.Code));
            }

            return;
        }
    }
}