using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowCreatedEventHandler : IHandleMessages<WorkflowCreatedEvent>
    {
        private readonly ITelegramService _telegramService;

        public WorkflowCreatedEventHandler(ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(WorkflowCreatedEvent message)
        {
            if (message.Order is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found!");

            if (message.Order.Modules.FirstOrDefault(x => x.Module.IsCustom) is not null)
            {
                await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.User}\" запущен новый процесс согласования!");
            }
            return;
        }
    }
}