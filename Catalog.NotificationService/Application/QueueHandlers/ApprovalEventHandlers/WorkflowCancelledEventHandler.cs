using Calabonga.UnitOfWork;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Events.Approval;
using Catalog.Domain.Entities;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowCancelledEventHandler : IHandleMessages<WorkflowCancelledEvent>
    {
        //private readonly ILogger<ComponentCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;

        public WorkflowCancelledEventHandler(ILogger<WorkflowCancelledEventHandler> logger, ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            //_logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
        }

        public async Task Handle(WorkflowCancelledEvent message)
        {
            var workflow = await _unitOfWork
                .GetRepository<ApprovalWorkflow>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.NoTracking,
                    include: query => query
                        .Include(x => x.OrderItem)
                            .ThenInclude(x => x.Order)
                        .Include(x => x.OrderItem)
                            .ThenInclude(x => x.Order)
                                .ThenInclude(x => x.ApplicationUser),
                    predicate: x => x.Code == message.WorkflowCode && x.Enabled);

            if (workflow is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. workflow not found! Code: {message.WorkflowCode}");

            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{workflow.OrderItem.Order.Title.Value}\" пользователя: \"{workflow.OrderItem.Order.ApplicationUser.UserName}\" завешен процесс согласования модуля!");

            return;
        }
    }
}
