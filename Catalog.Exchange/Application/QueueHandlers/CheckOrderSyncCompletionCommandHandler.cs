using Calabonga.UnitOfWork;
using Catalog.Contracts.ApplicationEvents;
using Catalog.ExchangeService.Application.Commands;
using Rebus.Handlers;

namespace Catalog.ExchangeService.Application.QueueHandlers
{
    public class CheckOrderSyncCompletionCommandHandler : IHandleMessages<CheckOrderSyncCompletionCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckOrderSyncCompletionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CheckOrderSyncCompletionCommand message)
        {
            var exchangeEvetRepository = _unitOfWork.GetRepository<ExchangeEvent>();

            var exchangeEvet = await exchangeEvetRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Id == message.TransactionId,
                    trackingType: TrackingType.Tracking);

            if (exchangeEvet is null || exchangeEvet.Enabled)
                return;

            exchangeEvetRepository.Delete(exchangeEvet);

            await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                throw new InvalidOperationException(_unitOfWork.Result.Exception.Message);
            }
        }
    }
}
