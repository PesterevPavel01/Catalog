using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.NotificationService.Application.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.OrderEventHandlers
{
    public sealed class OrderCreatedEventHandler : IHandleMessages<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger, ITelegramService telegramService, IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _logger = logger;
            _telegramService = telegramService;
            var approvalBotConfiguration = applicationConfiguration.Value.ApprovalNotificationBot;
            _telegramService.Initialize(token: approvalBotConfiguration.Token, chatId: approvalBotConfiguration.ChatId);
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(OrderCreatedEvent message)
        {
            /*
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.NoTracking,
                    include: Order.IncludeRequiredField(),
                    predicate: x => x.Code == message.OrderCode);

            if(order is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.OrderCode}");

            if(order.OrderItems.FirstOrDefault(x => x.Module.IsCustom == true) is not null)
                await _telegramService.SendMessageAsync($"Получен нестандартный заказ, который требует согласования конструктора. Код заказа: {message.OrderCode}");
            */
            return;
        }
    }
}
