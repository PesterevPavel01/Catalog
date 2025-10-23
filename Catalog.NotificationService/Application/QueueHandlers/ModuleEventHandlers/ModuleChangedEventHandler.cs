using Catalog.Contracts.Events;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ModuleEventHandlers
{
    public sealed class ModuleChangedEventHandler : IHandleMessages<ModuleChangedEvent>
    {
        private readonly ILogger<ModuleChangedEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public ModuleChangedEventHandler(ILogger<ModuleChangedEventHandler> logger, ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(ModuleChangedEvent message)
        {
            _logger.LogInformation("[{ServiceName}] Event {EventType} received successfully. Module ID: {ModuleId}",
                "ModuleConfigurationService".ToUpper(),
                message.GetType().Name,
                message.ModuleId);
            /*
            var result = await _telegramService.SendMessageAsync($"{"ModuleConfigurationService".ToUpper()} Event {message.GetType().Name} received successfully. Module ID: {message.ModuleId}");
            
            if(!result.Ok)
                _logger.LogError("[{ServiceName}]. {Message} Module ID: {ModuleId}",
                    "NotificationService".ToUpper(),
                    result.Error,
                    message.ModuleId);
            */
            return;
        }
    }
}
