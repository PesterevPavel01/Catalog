using Calabonga.UnitOfWork;
using Catalog.Contracts;
using Catalog.Contracts.Entities;
using MediatR;
using TelegramService.Interfaces;

namespace Catalog.Infrastructure;

public sealed class OutboxCleanerProcessor : IOutboxCleanerProcessor
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelegramService _telegramService;
    private readonly TimeProvider _timerProvider;

    public OutboxCleanerProcessor(
        IUnitOfWork unitOfWork, 
        ITelegramService telegramService,
        TimeProvider timerProvider)
    {
        _telegramService = telegramService;
        _unitOfWork = unitOfWork;
        _timerProvider = timerProvider;
    }

    public async Task ProcessAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var outboxMessageRepository = _unitOfWork
            .GetRepository<OutboxMessage>();

        var removeOutboxMessages = await outboxMessageRepository
            .GetAllAsync(
                predicate: x => x.ProcessedAt < _timerProvider.GetLocalNow().AddDays(-10),
                trackingType: TrackingType.Tracking);

        outboxMessageRepository.Delete(removeOutboxMessages);

        await _unitOfWork.SaveChangesAsync();

        if (!_unitOfWork.Result.Ok)
        {
            await _telegramService.SendMessageAsync($"{"OrderService".ToUpper()}.{typeof(OutboxCleanerProcessor).Name}. {_unitOfWork.Result.Exception}");
            return;
        }
    }
}
