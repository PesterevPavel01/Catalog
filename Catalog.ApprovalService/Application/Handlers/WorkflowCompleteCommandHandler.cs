using Catalog.Contracts.Events.ApprovalEvents;
using MediatR;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ApprovalService.Application.Handlers;

public class WorkflowCompleteCommandHandler : INotificationHandler<WorkflowCompleteCommand>
{
    private readonly IBus _bus;
    private readonly ITelegramService _telegramService;

    public WorkflowCompleteCommandHandler(ITelegramService telegramService, IBus bus, IOptions<TelegramBotConfiguration> telegramBotConfiguration)
    {
        _bus = bus;
        _telegramService = telegramService;
        _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
    }

    public async Task Handle(WorkflowCompleteCommand message, CancellationToken cancellationToken)
    {
        if (message.Order is null)
        {
            await _telegramService.SendMessageAsync($"{"ApprovalService".ToUpper()} Event {message.GetType().Name}. Order not found!");
            return;
        }

        await Task.Delay(5000, cancellationToken);

        await _bus.Publish(new WorkflowCompletedEvent(message.Order));
    }
}