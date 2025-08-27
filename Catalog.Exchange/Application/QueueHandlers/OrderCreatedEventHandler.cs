using Catalog.Contracts.Events;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.ExchangeService.Application.QueueHandlers
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
            _logger.LogInformation("[{ServiceName}] Event {EventType} received successfully. OrderCode {OrderCode}",
                "ExchangeService".ToUpper(),
                message.GetType().Name,
                message.OrderCode);

            //_telegramService.SendMessageAsync($"{"ExchangeService".ToUpper()} Event {message.GetType().Name} received successfully. OrderCode {message.OrderCode}").GetAwaiter().GetResult();

            return Task.CompletedTask;
        }
    }
}
