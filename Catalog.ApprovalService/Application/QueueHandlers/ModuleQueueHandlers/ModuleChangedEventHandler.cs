using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Processors;
using Catalog.Contracts.Events;
using Rebus.Handlers;

namespace Catalog.ApprovalService.Application.QueueHandlers.ModuleQueueHandlers
{
    public sealed class ModuleChangedEventHandler : IHandleMessages<ModuleChangedEvent>
    {
        private readonly ILogger<ModuleChangedEventHandler> _logger;

        private readonly ModuleApprovalWorkflowRestartProcessor _processor;
        
        public ModuleChangedEventHandler(ModuleApprovalWorkflowRestartProcessor processor, ILogger<ModuleChangedEventHandler> logger)
        {
            _processor = processor;
            _logger = logger;
        }

        public async Task Handle(ModuleChangedEvent message)
        {
            var result = await _processor.ProcessAsync(message.ModuleId, new CancellationToken());

            //Обработать ошибку!

            return;
        }
    }
}
