using Catalog.Contracts.Events.OrderEvents;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers
{
    public sealed class OrderCreatedEventHandler : IHandleMessages<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger;
        private readonly ITelegramService _telegramService;

        public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger, ITelegramService telegramService)
        {
            _logger = logger;
            _telegramService = telegramService;
        }

        public Task Handle(OrderCreatedEvent message)
        {
            _logger.LogInformation("[{ServiceName}] Event {EventType} recived successfully. OrderCode {OrderCode}",
                "OrderService".ToUpper(),
                message.GetType().Name,
                message.OrderCode);

            //_telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name} recived successfully. OrderCode {message.OrderCode}").GetAwaiter().GetResult();

            return Task.CompletedTask;
        }
    }
}
