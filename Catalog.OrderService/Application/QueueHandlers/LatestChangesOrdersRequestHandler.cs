using Catalog.Contracts.Request;
using Catalog.Contracts.Response;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers
{
    public sealed class LatestChangesOrdersRequestHandler : IHandleMessages<LatestChangesOrdersRequest>
    {
        private readonly LatestChangesOrdersQueryHandler _queryHandler;
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public LatestChangesOrdersRequestHandler(LatestChangesOrdersQueryHandler queryHandler, IBus bus, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _bus = bus;
            _queryHandler = queryHandler;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(LatestChangesOrdersRequest message)
        {
            var result = await _queryHandler.HandleAsync(message.lastExchangeDate, message.currentExchangeDate);

            if (!result.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: {result.Error}");
                return;
            }

            var response = LatestChangesOrdersResponse.Create(result.Result);

            if (!response.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: {response.Error}");
                return;
            }

            await _bus.Reply(response.Result);
        }
    }
}
