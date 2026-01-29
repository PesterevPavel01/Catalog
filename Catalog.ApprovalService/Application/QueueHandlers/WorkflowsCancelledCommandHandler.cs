using Catalog.ApprovalService.Application.Commands;
using Catalog.Contracts.Events.Approval;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ApprovalService.Application.QueueHandlers
{
    public class WorkflowsCancelledCommandHandler : IHandleMessages<CancelWorkflowCommand>
    {
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public WorkflowsCancelledCommandHandler(ITelegramService telegramService, IBus bus, IOptions<TelegramBotConfiguration> telegramBotConfiguration)
        {
            _bus = bus;
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
        }

        public async Task Handle(CancelWorkflowCommand message)
        {
            if (message.Order is null)
            {
                await _telegramService.SendMessageAsync($"{"ApprovalService".ToUpper()} Event {message.GetType().Name}. Order not found!");
                throw new ArgumentException($"{"ApprovalService".ToUpper()} Event {message.GetType().Name}. Order not found!");
            }
        
            await _bus.Publish(new WorkflowsCancelledEvent(message.Order));

            return;
        }
    }
}