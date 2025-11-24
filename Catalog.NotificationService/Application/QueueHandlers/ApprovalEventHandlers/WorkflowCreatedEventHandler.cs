using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Domain.Entities;
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
        private readonly IUnitOfWork _unitOfWork;

        public WorkflowCreatedEventHandler(ITelegramService telegramService, IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            //_logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(WorkflowCreatedEvent message)
        {
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.NoTracking,
                    include: Order.IncludeRequiredField(),
                    predicate: x => x.Code == message.OrderCode && x.Enabled);

            if (order is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.OrderCode}");

            //only for IsCustom orders
            if (order.OrderItems.FirstOrDefault(x => x.Module.IsCustom) is not null)
                await _telegramService.SendMessageAsync($"СОГЛАСОВАНИЕ ЗАКАЗА: у заказа \"{order.Title.Value}\" пользователя: \"{order.ApplicationUser.UserName}\" запущен новый процесс согласования!");

            return;
        }
    }
}