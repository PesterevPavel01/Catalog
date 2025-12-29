using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowRejectedEventHandler : IHandleMessages<WorkflowRejectedEvent>
    {
        private readonly ILogger<WorkflowRejectedEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;

        public WorkflowRejectedEventHandler(ILogger<WorkflowRejectedEventHandler> logger, ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(WorkflowRejectedEvent message)
        {
            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.UserName}\" не пройден процесс согласования модуля!");

            return;
        }
    }
}
