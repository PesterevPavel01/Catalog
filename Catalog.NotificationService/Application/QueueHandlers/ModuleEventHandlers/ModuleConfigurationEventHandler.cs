using Catalog.Contracts.Events;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ModuleEventHandlers
{
    public sealed class ModuleConfigurationEventHandler : IHandleMessages<ModuleCreatedEvent>
    {
        private readonly ILogger<ModuleConfigurationEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public ModuleConfigurationEventHandler(ILogger<ModuleConfigurationEventHandler> logger, ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(ModuleCreatedEvent message)
        {
            _logger.LogInformation("[{ServiceName}] Event {EventType} recived successfully. Module ID: {ModuleId}",
                "ModuleConfigurationService".ToUpper(),
                message.GetType().Name,
                message.ModuleId);

            var result = await _telegramService.SendMessageAsync($"{"ModuleConfigurationService".ToUpper()} Event {message.GetType().Name} recived successfully. Module ID: {message.ModuleId}");

            if(!result.Ok)
                _logger.LogError("[{ServiceName}]. {Message} Module ID: {ModuleId}",
                    "NotificationService".ToUpper(),
                    result.Error,
                    message.ModuleId);
        }
    }
}
