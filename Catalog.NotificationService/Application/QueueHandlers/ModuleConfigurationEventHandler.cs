using Catalog.Contracts.Events;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers
{
    public sealed class ModuleConfigurationEventHandler : IHandleMessages<ModuleCreatedEvent>
    {
        private readonly ILogger<ModuleConfigurationEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public ModuleConfigurationEventHandler(ILogger<ModuleConfigurationEventHandler> logger, ITelegramService telegramService)
        {
            _logger = logger;
            _telegramService = telegramService;
        }

        public async Task Handle(ModuleCreatedEvent message)
        {
            _logger.LogInformation("[{ServiceName}] Event {EventType} recived successfully. Order ID: {OrderId}, Module ID: {ModuleId}",
                "ModuleConfigurationService".ToUpper(),
                message.GetType().Name,
                message.OrderId,
            message.ModuleId);

            await _telegramService.SendMessageAsync($"{"ModuleConfigurationService".ToUpper()} Event {message.GetType().Name} recived successfully. Order ID: {message.OrderId}, Module ID: {message.ModuleId}");
        }
    }
}
