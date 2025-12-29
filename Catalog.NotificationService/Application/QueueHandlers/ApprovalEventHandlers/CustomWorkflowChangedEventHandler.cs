using Calabonga.UnitOfWork;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class CustomWorkflowChangedEventHandler : IHandleMessages<CustomWorkflowChangedEvent>
    {
        //private readonly ILogger<ComponentCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;

        public CustomWorkflowChangedEventHandler(ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            //_logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CustomWorkflowChangedEvent message)
        {
            var workflow = await _unitOfWork
                .GetRepository<ApprovalWorkflow>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.Tracking,
                    include: query => query
                        .Include(x => x.OrderItem)
                            .ThenInclude(x => x.Order)
                        .Include(x => x.OrderItem)
                            .ThenInclude(x => x.Order)
                                .ThenInclude(x => x.ApplicationUser),
                    predicate: x => x.Id == message.WorkflowId && x.Enabled);

            if (workflow is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. workflow not found! Id: {message.WorkflowId}");

            //only for IsCustom orders
            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{workflow.OrderItem.Order.Title.Value}\" пользователя: \"{workflow.OrderItem.Order.ApplicationUser.UserName}\" произошли изменения нестандартного модуля!");

            return;
        }
    }
}