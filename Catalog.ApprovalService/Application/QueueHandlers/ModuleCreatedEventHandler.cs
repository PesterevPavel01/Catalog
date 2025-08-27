using Catalog.Contracts.Events;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.ApprovalService.Application.QueueHandlers
{
    public sealed class ModuleCreatedEventHandler : IHandleMessages<ModuleCreatedEvent>
    {
        private readonly ILogger<ModuleCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IBus _bus;

        public ModuleCreatedEventHandler(ILogger<ModuleCreatedEventHandler> logger, ITelegramService telegramService, IBus bus)
        {
            _logger = logger;
            _telegramService = telegramService;
            _bus = bus;
        }

        public async Task Handle(ModuleCreatedEvent message)
        {
            _logger.LogInformation("[{ServiceName}] Event {EventType} received successfully. Order ID: {OrderId}, Module ID: ModuleId",
                "ApprovalService".ToUpper(),
                message.GetType().Name,
                message.OrderId,
                message.ModuleId);

            await Task.Delay(3000);

            await _bus.Publish(new ApprovalCompletedEvent(message.OrderId, message.ModuleId));
        }
    }
}
