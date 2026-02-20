using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.ExchangeEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ExportEventHandlers
{
    public class EntitiesExportedEventHandler : IHandleMessages<EntitiesExportedEvent>
    {
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramService _telegramService;

        public EntitiesExportedEventHandler(IBus bus, IUnitOfWork unitOfWork, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _bus = bus;
            _unitOfWork = unitOfWork;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(EntitiesExportedEvent message)
        {
            if (message.Entities.Type != typeof(Order).Name)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: Type not found!");
                return;
            }

            if (message.Entities is null) 
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: Orders not found!");
                return;
            }

            if (message.Entities.Codes is null || !message.Entities.Codes.Any())
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. OrderCodes not found!");
                return;
            }

            var orders = await _unitOfWork
                .GetRepository<Order>()
                .GetAllAsync(
                    predicate: x => message.Entities.Codes.Contains(x.Code),
                    include: Order.IncludeRequiredField());

            if (!orders.Any())
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: Order not found!");
                return;
            }

            foreach (var code in message.Entities.Codes)
            {
                var order = orders.FirstOrDefault(x => x.Code == code);

                if (order is null)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: Order not found! Code: {code}");
                    continue;
                }

                await _bus.Publish(new CreateOrderEventCommand(code, OrderEventType.Exported, OrderEventTypeTitles.Exported));
                await _bus.Publish(new OrderExportedEvent(order.ConvertToDto()));
            }
        }
    }
}
