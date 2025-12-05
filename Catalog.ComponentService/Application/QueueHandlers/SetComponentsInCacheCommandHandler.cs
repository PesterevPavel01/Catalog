using Catalog.ComponentService.Application.Command;
using Catalog.ComponentService.Application.Processors;
using Rebus.Handlers;
using TelegramService.Interfaces;

namespace Catalog.ComponentService.Application.QueueHandlers
{
    public class SetComponentsInCacheCommandHandler : IHandleMessages<SetComponentsInCacheCommand>
    {
        private readonly ILogger<SetComponentsInCacheCommandHandler> _logger;
        private readonly ITelegramService _telegramService;
        CachedComponentLoaderProcessor _cachedComponentLoaderProcessor;

        public SetComponentsInCacheCommandHandler(CachedComponentLoaderProcessor cachedComponentLoaderProcessor, ILogger<SetComponentsInCacheCommandHandler> logger, ITelegramService telegramService)
        {
            _logger = logger;
            _telegramService = telegramService;
            _cachedComponentLoaderProcessor = cachedComponentLoaderProcessor;
        }

        public async Task Handle(SetComponentsInCacheCommand message)
        {
            _logger.LogInformation("[{ServiceName}] Command {EventType} received successfully. CacheKey : {CacheKey}",
                "ComponentService".ToUpper(),
                message.GetType().Name,
                message.CacheKey);

            var sendToCacheResult = await _cachedComponentLoaderProcessor.SendToCacheAsync(message.CacheKey, message.Components, default);

            if (!sendToCacheResult.Ok)
            {
                _telegramService.SendMessageAsync($"{"ComponentService".ToUpper()} Error: {sendToCacheResult.Error}").GetAwaiter().GetResult();
            }
        }
    }
}
