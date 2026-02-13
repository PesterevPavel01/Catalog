using Catalog.ComponentService.Application.Managers;
using Catalog.Contracts.Commands.Exchange;
using Catalog.Contracts.Dto.Exchange;
using Catalog.Contracts.Events.ExchangeEvents;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ComponentService.Application.QueueHandlers
{
    public sealed class ComponentSyncCommandHandler : IHandleMessages<ComponentSyncCommand>
    {
        private readonly ITelegramService _telegramService;
        private readonly IBus _bus;
        private readonly ComponentSyncManager _componentSyncManager;

        public ComponentSyncCommandHandler( ITelegramService telegramService, ComponentSyncManager componentSyncManager,
             IOptions<TelegramBotConfiguration> telegramBotConfiguration, IBus bus)
        {
            _telegramService = telegramService;
            _telegramService.Initialize(token: telegramBotConfiguration.Value.Token, chatId: telegramBotConfiguration.Value.ChatId);
            _bus = bus;
            _componentSyncManager = componentSyncManager;
        }

        public async Task Handle(ComponentSyncCommand message)
        {
            var syncResult = await _componentSyncManager.SyncAsync(message.Components, default);

            if(!syncResult.Ok)
                _telegramService.SendMessageAsync($"{"ComponentService".ToUpper()} Error:{syncResult.Error}").GetAwaiter().GetResult();

            await _bus.Publish(new ComponentSyncCompletedEvent(new SyncConfirmationDto() { SyncSessionCode = message.SessionCode, RejectedEntities = syncResult.Result }));
        }
    }
}
