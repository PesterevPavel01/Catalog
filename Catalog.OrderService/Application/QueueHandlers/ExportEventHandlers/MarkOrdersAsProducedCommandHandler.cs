using Catalog.Contracts.Commands.Exchange;
using Catalog.OrderService.Application.Messages.OrderMessages;
using MediatR;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ExportEventHandlers;

public class MarkOrdersAsProducedCommandHandler : IHandleMessages<MarkOrdersAsProducedCommand>
{
    private readonly IMediator _mediator;
    private readonly ITelegramService _telegramService;

    public MarkOrdersAsProducedCommandHandler(ITelegramService telegramService,
        IOptions<TelegramBotConfiguration> exceptionNotificationBot, IMediator mediator)
    {
        _mediator = mediator;
        _telegramService = telegramService;
        _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
    }

    public async Task Handle(MarkOrdersAsProducedCommand message)
    {
        if (message.Codes is null || !message.Codes.Any())
            throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. OrderCode not found!");

        if (message.Codes is null || !message.Codes.Any())
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. OrderCodes not found!");
            return;
        }

        var result = await _mediator.Send(new CompleteProduction.Request(message.Codes), default);

        if (!result.Ok)
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()}.{typeof(EntitiesExportedEventHandler).Name}" +
                $" Errors:{result.Error.Select((error, index) => $"Error {index + 1}: {error}")}");
    }
}
