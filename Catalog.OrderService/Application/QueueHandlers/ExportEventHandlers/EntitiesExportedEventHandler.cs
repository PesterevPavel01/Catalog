using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.ExchangeEvents;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Messages.OrderMessages;
using MediatR;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using System;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers.ExportEventHandlers
{
    public class EntitiesExportedEventHandler : IHandleMessages<EntitiesExportedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ITelegramService _telegramService;

        public EntitiesExportedEventHandler(ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot, IMediator mediator)
        {
            _mediator = mediator;
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

            var result = await _mediator.Send(new SendToProduction.Request(message.Entities.Codes), default);

            if(!result.Ok)
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()}.{typeof(EntitiesExportedEventHandler).Name}" +
                    $" Errors:{result.Error.Select((error, index) => $"Error {index + 1}: {error}")}");
        }
    }
}
