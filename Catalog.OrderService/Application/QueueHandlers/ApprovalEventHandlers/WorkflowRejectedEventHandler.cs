using Catalog.Contracts.Commands;
using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Resources;
using Catalog.Redis;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public sealed class WorkflowRejectedEventHandler : IHandleMessages<WorkflowRejectedEvent>
    {
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public WorkflowRejectedEventHandler(IBus bus, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _bus = bus;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(WorkflowRejectedEvent message)
        {
            if (message.Order is null) 
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. Order not found!");
                return;
            }

            //only for IsCustom orders
            if (message.Order.Modules.FirstOrDefault(x => x.Module.IsCustom) is not null)
            {
                await _bus.Publish(new CreateOrderEventCommand(message.Order.Code, OrderEventTypes.Reject, OrderEventTypeTitles.Reject));
            }
            return;
        }
    }
}
