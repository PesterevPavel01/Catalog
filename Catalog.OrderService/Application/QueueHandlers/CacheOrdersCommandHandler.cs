using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Dto.Order;
using Catalog.OrderService.Application.Commands;
using Catalog.Redis;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers
{
    public class CacheOrdersCommandHandler : IHandleMessages<CacheOrdersCommand>
    {
        private readonly ITelegramService _telegramService;
        private readonly RedisService<OrderDto> _redisService;

        public CacheOrdersCommandHandler(RedisServiceFactory redisServiceFactory, ILogger<CacheOrderEventsCommandHandler> logger, ITelegramService telegramService)
        {
            _redisService = redisServiceFactory.GetService<OrderDto>();
            _telegramService = telegramService;
        }

        public async Task Handle(CacheOrdersCommand message)
        {
            var sendToCacheResult = await _redisService.SendToCacheAsync(message.CacheKey, message.Orders, default);

            if (!sendToCacheResult.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: {sendToCacheResult.Error}");
            }
        }
    }
}
