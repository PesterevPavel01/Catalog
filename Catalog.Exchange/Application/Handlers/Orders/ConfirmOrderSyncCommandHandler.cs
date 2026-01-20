using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.ApplicationEvents;
using Catalog.Contracts.Dto.Exchange;
using Catalog.Domain.Entities;
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

        public async Task<Operation<bool, string>> HandleAsync(string exchangeCode, CancellationToken cancellationToken = default) 
        {
            var exchangeEventRepository = _unitOfWork.GetRepository<ExchangeEvent>();

            var exchangeEvent = await exchangeEventRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == exchangeCode,
                    trackingType: TrackingType.Tracking);

            if (exchangeEvent is null)
                return Operation.Error("Exchange event not found");

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

            await _bus.Publish(new EntitiesExportedEvent(new ExportedEntitiesDto(typeof(Order).Name, exportedOrderCodes)));

            return true;
        }
    }
}
