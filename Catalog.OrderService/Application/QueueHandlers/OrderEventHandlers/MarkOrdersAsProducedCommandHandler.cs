using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Commands.Exchange;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.OrderEventHandlers
{
    public class MarkOrdersAsProducedCommandHandler : IHandleMessages<MarkOrdersAsProducedCommand>
    {
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelegramService _telegramService;

        public MarkOrdersAsProducedCommandHandler(IBus bus, IUnitOfWork unitOfWork, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _unitOfWork = unitOfWork;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
            _bus = bus;
        }

        public async Task Handle(MarkOrdersAsProducedCommand message)
        {
            if (message.codes is null || !message.codes.Any())
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. OrderCode not found!");

            var orders = await _unitOfWork
                .GetRepository<Order>()
                .GetAllAsync(
                    predicate: x => message.codes.Contains(x.Code),
                    include: Order.IncludeRequiredField());

            if (!orders.Any()) 
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: Order not found!");
                return;
            }

            foreach (var code in message.codes)
            {
                var order = orders.FirstOrDefault(x => x.Code == code);

                if (order is null)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: Order not found! Code: {code}");
                    continue;
                }

                await _bus.Publish(new CreateOrderEventCommand(code, OrderEventTypes.Produced, OrderEventTypeTitles.Produced));
                await _bus.Publish(new MarkOrdersAsProducedEvent(order.ConvertToDto()));
            }
        }
    }
}