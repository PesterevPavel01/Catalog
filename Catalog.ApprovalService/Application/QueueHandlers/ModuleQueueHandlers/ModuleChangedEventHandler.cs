using Catalog.ApprovalService.Application.Processors;
using Catalog.Contracts.Events;
using Catalog.Contracts.Events.ApprovalEvents;
using Rebus.Bus;
using Rebus.Handlers;

namespace Catalog.ApprovalService.Application.QueueHandlers.ModuleQueueHandlers
{
    public sealed class ModuleChangedEventHandler : IHandleMessages<ModuleChangedEvent>
    {        
        private readonly ModuleApprovalWorkflowRestartProcessor _processor;
        private readonly IBus _bus;

        public ModuleChangedEventHandler( IBus bus, ModuleApprovalWorkflowRestartProcessor processor)
        {
            _processor = processor;
            _bus = bus;
        }

        public async Task Handle(ModuleChangedEvent message)
        {
            var result = await _processor.ProcessAsync(message.ModuleId, new CancellationToken());

            if (result.Ok)
            {
                if(result.Result.IsCustom)
                    foreach(var item in result.Result.OrderItems)
                        await _bus.Publish(new CustomWorkflowChangedEvent(item.ApprovalWorkflow.Id));
            }

            return;
        }
    }
}
