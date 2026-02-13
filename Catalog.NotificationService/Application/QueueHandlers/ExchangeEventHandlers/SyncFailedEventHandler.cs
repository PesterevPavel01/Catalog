using Catalog.Contracts.Events.ExchangeEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ExchangeEventHandlers
{
    public class SyncFailedEventHandler : IHandleMessages<SyncFailedEvent>
    {
        private readonly ITelegramService _telegramService;

        public SyncFailedEventHandler(ITelegramService telegramService,
            IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _telegramService = telegramService;
            var approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalNotificationBot.Token, chatId: approvalNotificationBot.ChatId);
        }

        public async Task Handle(SyncFailedEvent message)
        {
            await _telegramService.SendMessageAsync(message.Message);
        }
    }
}