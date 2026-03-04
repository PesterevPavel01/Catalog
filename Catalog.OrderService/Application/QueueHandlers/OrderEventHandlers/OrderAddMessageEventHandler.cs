using Catalog.Contracts.Events.OrderEvents;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.OrderEventHandlers
{
    public class OrderAddMessageEventHandler : IHandleMessages<OrderAddMessageEvent>
    {
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public OrderAddMessageEventHandler(IBus bus, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _bus = bus;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(OrderAddMessageEvent message)
        {
            if (message.Order is null)
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order code not found!");

            await _bus.Publish(new UpdateOrderCacheCommand(message.Order.Code));
        }
    }
}