using Catalog.Contracts.Events.OrderEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.OrderEventHandlers
{
    public class OrderAddMessageEventHandler : IHandleMessages<OrderAddMessageEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly TelegramBotConfiguration _approvalNotificationBot;
        private readonly TelegramBotConfiguration _exceptionNotificationBot;

        public OrderAddMessageEventHandler(ITelegramService telegramService,
            IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _telegramService = telegramService;
            _approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
            _exceptionNotificationBot = exceptionNotificationBot.Value;
        }

        public async Task Handle(OrderAddMessageEvent message)
        {
            if (message.Order is null) 
            {
                _telegramService.Initialize(token: _exceptionNotificationBot.Token, chatId: _exceptionNotificationBot.ChatId);

                await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.Order.Code}");

                return;
            }

            _telegramService.Initialize(token: _approvalNotificationBot.Token, chatId: _approvalNotificationBot.ChatId);

            await _telegramService.SendMessageAsync($"НОВЫЙ КОММЕНТАРИЙ: к заказу \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" добавлен новый комментарий!");
        }
    }
}