using Catalog.Contracts.Events.CustomerEvents;
using Catalog.ExchangeService.Application.Handlers.Users;
using Microsoft.Extensions.Options;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ExchangeService.Application.QueueHandlers.EventHandlers
{
    public class CustomerCreatedEventHandler : IHandleMessages<CustomerCreatedEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly RefrashCacheUsersCommandHandler _refrashCacheUsersCommandHandler;

        public CustomerCreatedEventHandler(RefrashCacheUsersCommandHandler refrashCacheUsersCommandHandler, ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _refrashCacheUsersCommandHandler = refrashCacheUsersCommandHandler;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task Handle(CustomerCreatedEvent message)
        {
            var refreshCacheResult = await _refrashCacheUsersCommandHandler.HandleAsync(default);

            if (!refreshCacheResult.Ok)
                await _telegramService.SendMessageAsync($"{"ExchangeService".ToUpper()} Error: {refreshCacheResult.Error}");
        }
    }
}

