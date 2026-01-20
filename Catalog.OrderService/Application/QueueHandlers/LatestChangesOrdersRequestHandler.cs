using Catalog.Contracts.Request;
using Catalog.Contracts.Response;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers
{
    public sealed class LatestChangesOrdersRequestHandler : IHandleMessages<LatestChangesOrdersRequest>
    {
        private readonly LatestChangesOrdersQueryHandler _queryHandler;
        private readonly IBus _bus;
        private readonly ITelegramService _telegramService;

        public LatestChangesOrdersRequestHandler(LatestChangesOrdersQueryHandler queryHandler, IBus bus, ITelegramService telegramService)
        {
            _bus = bus;
            _queryHandler = queryHandler;
            _telegramService = telegramService;
        }

        public async Task Handle(LatestChangesOrdersRequest message)
        {
            var result = await _queryHandler.HandleAsync(message.lastExchangeDate, message.currentExchangeDate);

            var response = LatestChangesOrdersResponse.Create(result.Result);

            if (!response.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: {response.Error}");
                throw new ArgumentException($"{"OrderService".ToUpper()} Error: {response.Error}");
            }

            await _bus.Reply(response.Result);
        }
    }
}
