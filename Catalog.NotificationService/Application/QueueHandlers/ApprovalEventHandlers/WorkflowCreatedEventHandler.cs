using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowCreatedEventHandler : IHandleMessages<WorkflowCreatedEvent>
    {
        //private readonly ILogger<ComponentCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public WorkflowCreatedEventHandler(ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            //_logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(WorkflowCreatedEvent message)
        {
            await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name} received successfully. Создан заказ, который требует согласования: Code = \"{message.OrderCode}\"");

            return;
        }
    }
}