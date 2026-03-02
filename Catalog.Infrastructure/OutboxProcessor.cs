using Calabonga.UnitOfWork;
using Catalog.Contracts;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text.Json;
using TelegramService.Configurations;
using TelegramService.Interfaces;

namespace Catalog.Infrastructure
{
    public sealed class OutboxProcessor : IOutboxProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly TimeProvider _timerProvider;
        private readonly ITelegramService _telegramService;

        public OutboxProcessor(
            IUnitOfWork unitOfWork,
            IPublisher publisher,
            TimeProvider timerProvider,
            ITelegramService telegramService,
            IOptions<TelegramBotConfiguration> exceptionNotificationBot)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _timerProvider = timerProvider;
            _telegramService = telegramService;
            _telegramService.Initialize(token: exceptionNotificationBot.Value.Token, chatId: exceptionNotificationBot.Value.ChatId);
        }

        public async Task ProcessAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var pagedList = await _unitOfWork.GetRepository<OutboxMessage>()
                .GetPagedListAsync(
                    predicate: x => x.ProcessedAt == null,
                    pageIndex: 0,
                    pageSize: 20,
                    orderBy: o => o.OrderBy(x => x.CreatedAt),
                    trackingType: TrackingType.Tracking,
                    cancellationToken: cancellationToken
                );

            foreach (var message in pagedList.Items)
            {
                var domainEvent = JsonSerializer.Deserialize<IDomainEvent>(message.Content, OutboxMessage.JsonSettings);
                
                if (domainEvent is null)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()} Event {message.GetType().Name}. DomainEvent not found!");
                    continue;
                }

                await _publisher.Publish(domainEvent, cancellationToken);

                message.ProcessedAt = _timerProvider.GetLocalNow().DateTime;

                await _unitOfWork.SaveChangesAsync();

                if (!_unitOfWork.Result.Ok)
                {
                    await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()}.{typeof(OutboxProcessor).Name}. Event: {message.GetType().Name}. {_unitOfWork.Result.Exception}");
                    return;
                }
            }
        }
    }
}