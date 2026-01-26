using Catalog.Contracts.Events.OrderEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.OrderEventHandlers
{
    public class OrderAddMessageEventHandler : IHandleMessages<OrderAddMessageEvent>
    {
        private readonly ITelegramService _telegramService;

        public OrderAddMessageEventHandler(ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(OrderAddMessageEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.Order.Code}");

            if (message.Order.Modules.FirstOrDefault(x => x.Module.IsCustom) is not null)
            {
                await _telegramService.SendMessageAsync($"НОВЫЙ КОММЕНТАРИЙ: к заказу \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" добавлен новый комментарий!");
            }
            return;
        }
    }
}