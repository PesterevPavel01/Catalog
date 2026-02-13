using Calabonga.UnitOfWork;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class CustomWorkflowChangedEventHandler : IHandleMessages<CustomWorkflowChangedEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TelegramBotConfiguration _approvalNotificationBot;
        private readonly TelegramBotConfiguration _exceptionNotificationBot;

        public CustomWorkflowChangedEventHandler(ITelegramService telegramService, IUnitOfWork unitOfWork, 
            IOptions<ApplicationConfiguration> applicationConfiguration, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _approvalNotificationBot = applicationConfiguration.Value.ApprovalNotificationBot;
            _exceptionNotificationBot = exceptionNotificationBot.Value;
            _telegramService = telegramService;
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
            {
                _telegramService.Initialize(token: _exceptionNotificationBot.Token, chatId: _exceptionNotificationBot.ChatId);

                await _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. workflow not found! Id: {message.WorkflowId}");

                return;
            }

            _telegramService.Initialize(token: _approvalNotificationBot.Token, chatId: _approvalNotificationBot.ChatId);

            await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{workflow.OrderItem.Order.Title.Value}\" пользователя: \"{workflow.OrderItem.Order.ApplicationUser.UserName}\" произошли изменения нестандартного модуля!");

            return;
        }
    }
}