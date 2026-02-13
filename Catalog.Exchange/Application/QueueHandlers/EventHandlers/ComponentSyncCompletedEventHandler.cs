using Calabonga.UnitOfWork;
using Catalog.Contracts.Entities.Exchange;
using Catalog.Contracts.Events.ExchangeEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Handlers;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.ExchangeService.Application.QueueHandlers.EventHandlers
{
    public class ComponentSyncCompletedEventHandler : IHandleMessages<ComponentSyncCompletedEvent>
    {
        private readonly ITelegramService _telegramService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;

        public ComponentSyncCompletedEventHandler(ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot, IUnitOfWork unitOfWork,
            IBus bus)
        {
            _unitOfWork = unitOfWork;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
            _bus = bus;
        }

        public async Task Handle(ComponentSyncCompletedEvent message)
        {
            var exchangeEvetRepository = _unitOfWork.GetRepository<ExchangeEvent>();

            var exchangeEvet = await exchangeEvetRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == message.result.SyncSessionCode,
                    include: query => query.Include(x => x.Entities),
                    trackingType: TrackingType.Tracking);

            if (exchangeEvet is null || exchangeEvet.Enabled)
            {
                await _telegramService.SendMessageAsync($"{"ExchangeService".ToUpper()} Error: Exchange Event (Code: {message.result.SyncSessionCode}) not found!");
                return;
            }

            if (message.result.RejectedEntities.Any())
            {
                exchangeEvet.SetMessage("Failed");

                var failedEntities = exchangeEvet.Entities
                    .Where(x => message.result.RejectedEntities
                        .Select(e => e.Code)
                        .Contains(x.Code));

                foreach (var entity in failedEntities)
                {
                    var errors = message.result.RejectedEntities.Where(x => x.Code == entity.Code);

                    string errorMessage = string.Join("; ", errors
                        .Where(e => !string.IsNullOrWhiteSpace(e.Error))
                        .Select(e => e.Error));

                    if (!string.IsNullOrWhiteSpace(errorMessage))
                        entity.SetErrorMessage(errorMessage);

                    await _telegramService.SendMessageAsync($"{"ExchangeService".ToUpper()} Error: Component Sync filed! Session Code: {message.result.SyncSessionCode}, Component Code: {entity.Code}" +
                        $", Error: {errorMessage}");

                    await _bus.Publish(new SyncFailedEvent($"{exchangeEvet.Type}! {"Ошибка при обмене данными!".ToUpper()} Сессия: {message.result.SyncSessionCode}, Сообщение:  {errorMessage}"));
                }
            }
            else
            {
                exchangeEvet.SetMessage("Successfully completed");
            }

            await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await _telegramService.SendMessageAsync($"{"ExchangeService".ToUpper()} Error: {_unitOfWork.Result.Exception.Message}");
                return;
            }
        }
    }
}