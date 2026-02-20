using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Exchange;
using Catalog.Contracts.Entities.Exchange;
using Catalog.Contracts.Events.ExchangeEvents;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Events;
using MediatR;
using Rebus.Bus;

namespace Catalog.ExchangeService.Application.Messaging.OrderSyncMessages
{
    public sealed class ConfirmOrderSync
    {
        public record Request(SyncConfirmationDto SyncResult) : IRequest<Operation<bool, string>>;

        public class Handler(IUnitOfWork UnitOfWork, IBus Bus)
            : IRequestHandler<Request, Operation<bool, string>>
        {

            public async Task<Operation<bool, string>> Handle(Request request, CancellationToken cancellationToken = default)
            {
                var exchangeEventRepository = UnitOfWork.GetRepository<ExchangeEvent>();

                var exchangeEvent = await exchangeEventRepository
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == request.SyncResult.SyncSessionCode,
                        trackingType: TrackingType.Tracking);

                if (exchangeEvent is null)
                    return Operation.Error("Session not found!");

                exchangeEvent.Confirm();

                await UnitOfWork.SaveChangesAsync();

                if (UnitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(UnitOfWork.Result.Exception.Message);
                }

                var exportedOrderCodes = await UnitOfWork.GetRepository<ExportedEntity>()
                    .GetAllAsync(
                        predicate: x => x.ExchangeEvent.Id == exchangeEvent.Id,
                        trackingType: TrackingType.NoTracking,
                        selector: x => x.Code
                    );

                var successfullySyncedCodes = exportedOrderCodes.Where(x => !request.SyncResult.RejectedEntities.Select(e => e.Code).Contains(x));

                if (successfullySyncedCodes.Any())
                {

                    await Bus.Publish(new EntitiesExportedEvent(new ExportedEntitiesDto(typeof(Order).Name, successfullySyncedCodes)));

                    if (request.SyncResult.SuccessfullySynced.Any())
                        await Bus.DeferLocal(TimeSpan.FromMinutes(1), new SuccessfullySyncedEntitiesEvent(request.SyncResult.SuccessfullySynced));

                }

                if (request.SyncResult.RejectedEntities.Any())
                    await Bus.Publish(new RejectedEntitiesEvent(new ExportedEntitiesDto(typeof(Order).Name, request.SyncResult.RejectedEntities.Select(e => e.Code))));

                return true;
            }
        }
    }
}
