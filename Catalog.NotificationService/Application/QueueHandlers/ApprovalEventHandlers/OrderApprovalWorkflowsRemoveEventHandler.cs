using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class OrderApprovalWorkflowsRemoveEventHandler : IHandleMessages<OrderApprovalWorkflowsRemoveEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly TelegramBotConfiguration _approvalNotificationBot;
        private readonly TelegramBotConfiguration _exceptionNotificationBot;

        public OrderApprovalWorkflowsRemoveEventHandler(ILogger<OrderApprovalWorkflowsRemoveEventHandler> logger, ITelegramService telegramService,
            IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
            _exceptionNotificationBot = exceptionNotificationBot.Value;
            _telegramService = telegramService;
        }

        public async Task Handle(OrderApprovalWorkflowsRemoveEvent message)
        {
            _telegramService.Initialize(token: _approvalNotificationBot.Token, chatId: _approvalNotificationBot.ChatId);

            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" отменены все процессы согласования!");

            return;
        }
    }
}
