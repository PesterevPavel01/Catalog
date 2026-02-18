using Catalog.Contracts.Events.OrderEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.OrderEventHandlers
{
    public class MarkOrdersAsProducedEventHandler : IHandleMessages<MarkOrdersAsProducedEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly TelegramBotConfiguration _approvalNotificationBot;
        private readonly TelegramBotConfiguration _exceptionNotificationBot;

        public MarkOrdersAsProducedEventHandler(ITelegramService telegramService,
            IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _telegramService = telegramService;
            _approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
            _exceptionNotificationBot = exceptionNotificationBot.Value;
        }

        public async Task Handle(MarkOrdersAsProducedEvent message)
        {
            if (message.Order is null)
            {
                _telegramService.Initialize(token: _exceptionNotificationBot.Token, chatId: _exceptionNotificationBot.ChatId);

                await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.Order.Code}");

                return;
            }

            _telegramService.Initialize(token: _approvalNotificationBot.Token, chatId: _approvalNotificationBot.ChatId);

            await _telegramService.SendMessageAsync($"ПРОИЗВОДСТВО ЗАВЕРШЕНО: заказ \"{message.Order.Title}\" пользователя: \"{message.Order.User}\".");
        }
    }
}