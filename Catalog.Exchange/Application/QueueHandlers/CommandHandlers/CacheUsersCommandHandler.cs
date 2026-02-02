using Catalog.Contracts.Dto.Authorization;
using Catalog.ExchangeService.Application.Commands.Cache;
using Catalog.Redis;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ExchangeService.Application.QueueHandlers.CommandHandlers
{

    public class CacheUsersCommandHandler : IHandleMessages<CacheUsersCommand>
    {
        private readonly ITelegramService _telegramService;
        private readonly RedisService<UserDto> _redisService;

        public CacheUsersCommandHandler(RedisServiceFactory redisServiceFactory, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _redisService = redisServiceFactory.GetService<UserDto>();
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(CacheUsersCommand message)
        {
            var sendToCacheResult = await _redisService.SendToCacheAsync(message.CacheKey, message.Users, default);

            if (!sendToCacheResult.Ok)
            {
                await _telegramService.SendMessageAsync($"{"ExchangeService".ToUpper()} Error: {sendToCacheResult.Error}");
            }
        }
    }
}

