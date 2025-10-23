using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Services;
using Catalog.Contracts.Events.OrderEvents;
using Rebus.Handlers;

namespace Catalog.ApprovalService.Application.QueueHandlers.ModuleQueueHandlers
{
    public sealed class OrderItemIncludedEventHandler : IHandleMessages<OrderItemsIncludedEvent>
    {
        private readonly ILogger<ModuleChangedEventHandler> _logger;

        private readonly IUnitOfWork _unitOfWork;

        private readonly OrderItemApprovalInitiatorService _processor;
        
        public OrderItemIncludedEventHandler(OrderItemApprovalInitiatorService processor, ILogger<ModuleChangedEventHandler> logger)
        {
            _processor = processor;
            _logger = logger;
        }

        public async Task Handle(OrderItemsIncludedEvent message)
        {

            var result = await _processor.InitializeAsync(message.models, new CancellationToken());

            return;
        }
    }
}
