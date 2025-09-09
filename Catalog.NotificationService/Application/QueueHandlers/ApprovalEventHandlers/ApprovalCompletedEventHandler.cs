using Catalog.Contracts.Events.Approval;
using Catalog.NotificationService.Application.Configurations;
using Catalog.NotificationService.Application.QueueHandlers.ComponentEventHandlers;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public sealed class ApprovalCompletedEventHandler : IHandleMessages<ApprovalCompletedEvent>
    {
        private readonly ILogger<ComponentCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public ApprovalCompletedEventHandler(ILogger<ComponentCreatedEventHandler> logger, ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(ApprovalCompletedEvent message)
        {
            _logger
                .LogInformation("[{ServiceName}] Event {EventType} received successfully. Approved module ID: {ModuleId}",
                    "NotificationService".ToUpper(),
                    message.GetType().Name,
                    message.ModuleId);

            await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name} received successfully.  Approved custom module ID: {message.ModuleId}");

            return;
        }
    }
}
