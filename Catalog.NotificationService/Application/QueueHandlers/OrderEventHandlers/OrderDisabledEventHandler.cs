using Catalog.Contracts.Events.OrderEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.OrderEventHandlers
{
    public class OrderDisabledEventHandler : IHandleMessages<OrderDisabledEvent>
    {
        private readonly ITelegramService _telegramService;

        public OrderDisabledEventHandler(ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(OrderDisabledEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.Order.Code}");

            await _telegramService.SendMessageAsync($"ЗАКАЗ УДАЛЕН: заказ \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" был удален!");
        }
    }
}