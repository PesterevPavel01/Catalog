using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class OrderApprovalWorkflowsRemoveEventHandler : IHandleMessages<OrderApprovalWorkflowsRemoveEvent>
    {
        private readonly ILogger<OrderApprovalWorkflowsRemoveEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;

        public OrderApprovalWorkflowsRemoveEventHandler(ILogger<OrderApprovalWorkflowsRemoveEventHandler> logger, ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(OrderApprovalWorkflowsRemoveEvent message)
        {
            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{message.Order.Title}\" пользователя: \"{message.Order.UserName}\" отменены все процессы согласования!");

            return;
        }
    }
}
