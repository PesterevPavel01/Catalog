using Catalog.Contracts.Events.Approval;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowCancelledEventHandler : IHandleMessages<WorkflowsCancelledEvent>
    {
        private readonly ITelegramService _telegramService;

        public WorkflowCancelledEventHandler(ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(WorkflowsCancelledEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. order not found! Code: {message.Order.Code}");

            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.UserName}\" завершен процесс согласования модуля!");

            return;
        }
    }
}
