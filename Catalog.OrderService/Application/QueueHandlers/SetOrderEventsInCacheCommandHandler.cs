using Catalog.Contracts.Dto.Events;
using Catalog.OrderService.Application.Commands;
using Catalog.Redis;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers
{
    public class SetOrderEventsInCacheCommandHandler : IHandleMessages<SetOrderEventsInCacheCommand>
    {
        private readonly ITelegramService _telegramService;
        private readonly RedisService<OrderEventDto> _redisService;

        public SetOrderEventsInCacheCommandHandler(RedisServiceFactory redisServiceFactory, ILogger<SetOrderEventsInCacheCommandHandler> logger, ITelegramService telegramService)
        {
            _redisService = redisServiceFactory.GetService<OrderEventDto>();
            _telegramService = telegramService;
        }

        public async Task Handle(SetOrderEventsInCacheCommand message)
        {
            var sendToCacheResult = await _redisService.SendToCacheAsync(message.CacheKey, message.Events, default);

            if (!sendToCacheResult.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: {sendToCacheResult.Error}");
            }
        }
    }
}
