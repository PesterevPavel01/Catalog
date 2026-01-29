using Catalog.Contracts.Events.Approval;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowCancelledEventHandler : IHandleMessages<WorkflowsCancelledEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly TelegramBotConfiguration _approvalNotificationBot;
        private readonly TelegramBotConfiguration _exceptionNotificationBot;

        public WorkflowCancelledEventHandler(ITelegramService telegramService,
            IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _telegramService = telegramService;
            _approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
            _exceptionNotificationBot = exceptionNotificationBot.Value;
        }

        public async Task Handle(WorkflowsCancelledEvent message)
        {
            if (message.Order is null) {

                _telegramService.Initialize(token: _exceptionNotificationBot.Token, chatId: _exceptionNotificationBot.ChatId);

                await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. order not found! Code: {message.Order.Code}");

                return;
            }

            _telegramService.Initialize(token: _approvalNotificationBot.Token, chatId: _approvalNotificationBot.ChatId);

            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" завершен процесс согласования модуля!");

            return;
        }
    }
}
