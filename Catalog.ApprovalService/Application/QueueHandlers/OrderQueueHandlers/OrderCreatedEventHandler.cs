using Catalog.ApprovalService.Application.Processors;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Events.OrderEvents;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.ApprovalService.Application.QueueHandlers.OrderQueueHandlers
{
    public class OrderCreatedEventHandler : IHandleMessages<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger;
        private readonly ApprovalWorkflowInitiatorProcessor _workflowInitiatorProcessor;
        private readonly IBus _bus;

        public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger, ApprovalWorkflowInitiatorProcessor workflowInitiatorProcessor, IBus bus)
        {
            _logger = logger;
            _workflowInitiatorProcessor = workflowInitiatorProcessor;
            _bus = bus;
        }

        public async Task Handle(OrderCreatedEvent message)
        {
            var result = await _workflowInitiatorProcessor.ProcessAsync(message.OrderCode, new CancellationToken());

            if(!result.Ok)
                return;//тут надо отправить уведомление о ошибке

            if (result.Result.Any())
                await _bus.Publish(new WorkflowCreatedEvent(message.OrderCode));
        }
    }
}
