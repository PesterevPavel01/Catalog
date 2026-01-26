using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.ApplicationEvents;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Request;
using Catalog.Contracts.Response;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Commands;
using Rebus;
using Rebus.Bus;

namespace Catalog.ExchangeService.Application.Handlers.Orders
{
    public class GetLatestChangesOrdersCommandHandler
    {
        private readonly IBus _bus;
        private readonly IUnitOfWork _unitOfWork;
        public GetLatestChangesOrdersCommandHandler(IBus bus, IUnitOfWork unitOfWork)
        {
            _bus = bus;
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<LatestChangesOrderDto, string>> HandleAsync(string exchangeTypeTitle, CancellationToken cancellationToken = default) 
        {
            var exchangeEventRepository = _unitOfWork.GetRepository<ExchangeEvent>();

            var lastExchange = await exchangeEventRepository
                .GetFirstOrDefaultAsync(
                    predicate:x => x.Enabled && x.Type == exchangeTypeTitle,
                    orderBy: x => x.OrderByDescending(e => e.ExecutedAt));

            DateTime lastExchangeDate = default;

            if (lastExchange is not null)
                lastExchangeDate = lastExchange.ExecutedAt;

            var currentExchangeEvent = ExchangeEvent.Create(exchangeTypeTitle, DateTime.Now);

            if (!currentExchangeEvent.Ok)
                return Operation.Error(currentExchangeEvent.Error);

            var result = new LatestChangesOrdersRequest(lastExchangeDate, currentExchangeEvent.Result.ExecutedAt);

            var response = await _bus.SendRequest<LatestChangesOrdersResponse>(
                result,
                timeout: TimeSpan.FromSeconds(15),
                externalCancellationToken: cancellationToken);

            var orders = response.GetLatestChangesOrders();

            if(!orders.Ok)
                return Operation.Error(orders.Error);

            var exportedOrders = orders.Result.Orders.Select(x => ExportedEntity.Create(currentExchangeEvent.Result, typeof(Order).Name, x.Code));

            await _unitOfWork.GetRepository<ExportedEntity>().InsertAsync(exportedOrders, cancellationToken);

            await exchangeEventRepository.InsertAsync(currentExchangeEvent.Result, cancellationToken);

            await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            await _bus.DeferLocal(TimeSpan.FromMinutes(5), new CheckOrderSyncCompletionCommand(currentExchangeEvent.Result.Id));

            orders.Result.Code = currentExchangeEvent.Result.Code;

            return orders;
        }
    }
}
