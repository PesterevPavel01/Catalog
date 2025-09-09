using Calabonga.UnitOfWork;
using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.ApprovalService.Application.QueueHandlers.ModuleQueueHandlers
{
    public sealed class ModuleCreatedEventHandler : IHandleMessages<ModuleCreatedEvent>
    {
        private readonly ILogger<ModuleCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;

        public ModuleCreatedEventHandler(IUnitOfWork unitOfWork, ILogger<ModuleCreatedEventHandler> logger, ITelegramService telegramService, IBus bus)
        {
            _logger = logger;
            _telegramService = telegramService;
            _bus = bus;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(ModuleCreatedEvent message)
        {
            var module = await _unitOfWork
                .GetRepository<Module>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Id == message.ModuleId,
                    trackingType: TrackingType.NoTracking,
                    include: Module.IncludeRequaredField());

            if (module == null)
                return;
            /*
            if (module.IsCustom)
                await _bus.Publish(new ApprovalCompletedEvent(message.ModuleId));
            */
        }
    }
}
