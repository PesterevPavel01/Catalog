using Calabonga.UnitOfWork;
using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers
{
    public sealed class OrderCreatedEventHandler : IHandleMessages<OrderCreatedEvent>
    {
        private readonly ILogger<ComponentCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreatedEventHandler(ILogger<ComponentCreatedEventHandler> logger, ITelegramService telegramService, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _telegramService = telegramService;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(OrderCreatedEvent message)
        {
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.NoTracking,
                    include: Order.IncludeRequaredField(),
                    predicate: x => x.Code == message.OrderCode);

            if(order is null)
                throw new ArgumentException($"{"NotificationService".ToUpper()} Event {message.GetType().Name}. Order not found! Code: {message.OrderCode}");

            if(order.OrderItems.FirstOrDefault(x => x.Module.IsCostom == true) is not null)
                await _telegramService.SendMessageAsync($"Получен нестандартный заказ, который требует согласования конструктора. Код заказа: {message.OrderCode}");

            return;
        }
    }
}
