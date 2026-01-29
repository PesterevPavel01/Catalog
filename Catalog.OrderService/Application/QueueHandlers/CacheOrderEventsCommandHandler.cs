using Catalog.Contracts.Dto.Events;
using Catalog.OrderService.Application.Commands;
using Catalog.Redis;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.OrderService.Application.QueueHandlers
{
    public class CacheOrderEventsCommandHandler : IHandleMessages<CacheOrderEventsCommand>
    {
        private readonly ITelegramService _telegramService;
        private readonly RedisService<OrderEventDto> _redisService;

        public CacheOrderEventsCommandHandler(RedisServiceFactory redisServiceFactory, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _redisService = redisServiceFactory.GetService<OrderEventDto>();
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(CacheOrderEventsCommand message)
        {
            var sendToCacheResult = await _redisService.SendToCacheAsync(message.CacheKey, message.Events, default);

            if (!sendToCacheResult.Ok)
            {
                await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Error: {sendToCacheResult.Error}");
            }
        }
    }
}
