using Calabonga.UnitOfWork;
using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.ApprovalService.Application.QueueHandlers
{
    public class ModuleUpdatedEventHandler : IHandleMessages<ModuleUpdatedEvent>
    {
        private readonly ILogger<ModuleUpdatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;

        public ModuleUpdatedEventHandler(IUnitOfWork unitOfWork, ILogger<ModuleUpdatedEventHandler> logger, ITelegramService telegramService, IBus bus)
        {
            _logger = logger;
            _telegramService = telegramService;
            _bus = bus;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ModuleUpdatedEvent message)
        {
            var module = await _unitOfWork
                .GetRepository<Module>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Id == message.ModuleId,
                    trackingType: TrackingType.NoTracking,
                    include: Module.IncludeRequaredField());

            if (module == null) 
            return;

            await Task.Delay(3000);

            if(module.IsCostom)
                await _bus.Publish(new ApprovalCompletedEvent(message.ModuleId));

        }
    }
}
