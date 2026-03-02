using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.OrderEventHandlers
{
    public class OrderCreatedEventHandler : IHandleMessages<OrderCreatedEvent>
    {
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public OrderCreatedEventHandler(IBus bus, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _bus = bus;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(OrderCreatedEvent message)
        {
            if (String.IsNullOrWhiteSpace(message.OrderCode))
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order code not found!");

            await _bus.Publish(new UpdateOrderCacheCommand(message.OrderCode));
        }
    }
}