using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.Approval;
using Catalog.Contracts.Resources;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.NotificationService.Application.QueueHandlers.ApprovalEventHandlers
{
    public class WorkflowsCancelledEventHandler : IHandleMessages<WorkflowsCancelledEvent>
    {
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public WorkflowsCancelledEventHandler(IBus bus, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _bus = bus;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(WorkflowsCancelledEvent message)
        {
            if (message.Order is null) 
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. order not found! Code: {message.Order.Code}");
                return;
            }

            await _bus.Publish(new CreateOrderEventCommand(message.Order.Code, OrderEventType.ApprovalCompleted, OrderEventTypeTitles.ApprovalCompleted));

            return;
        }
    }
}
