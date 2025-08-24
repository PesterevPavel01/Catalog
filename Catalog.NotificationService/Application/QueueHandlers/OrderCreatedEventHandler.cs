using Catalog.Contracts.Events;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers
{
    public sealed class OrderCreatedEventHandler : IHandleMessages<OrderCreatedEvent>
    {
        private readonly ILogger<ComponentCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public OrderCreatedEventHandler(ILogger<ComponentCreatedEventHandler> logger, ITelegramService telegramService)
        {
            _logger = logger;
            _telegramService = telegramService;
        }

        public Task Handle(OrderCreatedEvent message)
        {
            _logger.LogInformation("[{ServiceName}] Event {EventType} received successfully. OrderCode {OrderCode}",
                "NotificationService".ToUpper(),
                message.GetType().Name,
                message.OrderCode);

            _telegramService.SendMessageAsync($"{"NotificationService".ToUpper()} Event {message.GetType().Name} received successfully. OrderCode {message.OrderCode}").GetAwaiter().GetResult();

            return Task.CompletedTask;
        }
    }
}
