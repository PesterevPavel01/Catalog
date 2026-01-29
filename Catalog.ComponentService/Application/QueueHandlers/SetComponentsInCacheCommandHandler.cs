using Catalog.ComponentService.Application.Commands;
using Catalog.Contracts.Dto.Components;
using Catalog.Redis;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ComponentService.Application.QueueHandlers
{
    public class SetComponentsInCacheCommandHandler : IHandleMessages<SetComponentsInCacheCommand>
    {
        private readonly ILogger<SetComponentsInCacheCommandHandler> _logger;
        private readonly ITelegramService _telegramService;
        private readonly RedisService<ComponentDto> _redisService;

        public SetComponentsInCacheCommandHandler(RedisServiceFactory redisServiceFactory, ILogger<SetComponentsInCacheCommandHandler> logger, ITelegramService telegramService,
             IOptions<TelegramBotConfiguration> telegramBotConfiguration)
        {
            _redisService = redisServiceFactory.GetService<ComponentDto>();
            _logger = logger;
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
        }

        public async Task Handle(SetComponentsInCacheCommand message)
        {
            _logger.LogInformation("[{ServiceName}] Command {EventType} received successfully. CacheKey : {CacheKey}",
                "ComponentService".ToUpper(),
                message.GetType().Name,
                message.CacheKey);

            var sendToCacheResult = await _redisService.SendToCacheAsync(message.CacheKey, message.Components, default);

            if (!sendToCacheResult.Ok)
            {
                _telegramService.SendMessageAsync($"{"ComponentService".ToUpper()} Error: {sendToCacheResult.Error}").GetAwaiter().GetResult();
            }
        }
    }
}
