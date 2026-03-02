using Catalog.ApprovalService.Application.Commands;
using Catalog.Contracts.Events.ApprovalEvents;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ApprovalService.Application.QueueHandlers
{
    public class WorkflowCreatedEventHandler : IHandleMessages<WorkflowCreatedEvent>
    {
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public WorkflowCreatedEventHandler(ITelegramService telegramService, IBus bus, IOptions<TelegramBotConfiguration> telegramBotConfiguration)
        {
            _bus = bus;
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
        }

        public async Task Handle(WorkflowCreatedEvent message)
        {
            if (message.Order is null)
            {
                await _telegramService.SendMessageAsync($"{"ApprovalService".ToUpper()} Event {message.GetType().Name}. Order not found!");
            }

            if (!message.Order.IsCustom)
            {
                await _bus.DeferLocal(TimeSpan.FromSeconds(5), new CancelWorkflowCommand(message.Order));
            }
            return;
        }
    }
}