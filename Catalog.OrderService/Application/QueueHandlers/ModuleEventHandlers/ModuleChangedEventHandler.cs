using Catalog.Contracts.Events;
using Catalog.OrderService.Application.Messages.OrderItemMessages;
using MediatR;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ModuleEventHandlers
{
    public class ModuleChangedEventHandler : IHandleMessages<ModuleChangedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ITelegramService _telegramService;

        public ModuleChangedEventHandler(IMediator mediator,
            ITelegramService telegramService, IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _mediator = mediator;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(ModuleChangedEvent message)
        {
            var result = await _mediator.Send(new UpdateModule.Request(message.ModuleCode), default);

            if (!result.Ok)
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()}. Event: {typeof(ModuleChangedEventHandler).Name}. Errors: {string.Join("; ", result.Error)}");

            return;
        }
    }
}
