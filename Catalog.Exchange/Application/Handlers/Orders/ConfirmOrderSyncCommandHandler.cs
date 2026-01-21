using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.ApplicationEvents;
using Catalog.Contracts.Dto.Exchange;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Events;
using Rebus.Bus;

namespace Catalog.ExchangeService.Application.Handlers.Orders
{
    public class ConfirmOrderSyncCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;

        public ConfirmOrderSyncCommandHandler(IUnitOfWork unitOfWork, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _bus = bus;
        }

        public async Task<Operation<bool, string>> HandleAsync(SyncConfirmationDto syncResult, CancellationToken cancellationToken = default) 
        {
            var exchangeEventRepository = _unitOfWork.GetRepository<ExchangeEvent>();

            var exchangeEvent = await exchangeEventRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == syncResult.SyncSessionCode,
                    trackingType: TrackingType.Tracking);

            if (exchangeEvent is null)
                return Operation.Error("Session not found!");

            exchangeEvent.Confirm();

            await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            var exportedOrderCodes = await _unitOfWork.GetRepository<ExportedEntity>()
                .GetAllAsync(
                    predicate: x => x.ExchangeEvent.Id == exchangeEvent.Id,
                    trackingType: TrackingType.NoTracking,
                    selector: x => x.Code
                );

            var successfullySyncedCodes = exportedOrderCodes.Where(x => !syncResult.RejectedCodes.Contains(x));

            if (successfullySyncedCodes.Any())
            {

                await _bus.Publish(new EntitiesExportedEvent(new ExportedEntitiesDto(typeof(Order).Name, successfullySyncedCodes)));

                if(syncResult.SuccessfullySynced.Any())
                    await _bus.DeferLocal(TimeSpan.FromMinutes(1), new SuccessfullySyncedEntitiesEvent(syncResult.SuccessfullySynced));

            }

            if(syncResult.RejectedCodes.Any())
                await _bus.Publish(new RejectedEntitiesEvent(new ExportedEntitiesDto(typeof(Order).Name, syncResult.RejectedCodes)));

            return true;
        }
    }
}
