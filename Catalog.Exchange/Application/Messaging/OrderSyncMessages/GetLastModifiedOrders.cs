using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities.Exchange;
using Catalog.Contracts.Request;
using Catalog.Contracts.Response;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Commands;
using MediatR;
using Rebus;
using Rebus.Bus;

namespace Catalog.ExchangeService.Application.Messaging.OrderSyncMessages
{
    public sealed class GetLastModifiedOrders
    {
        public record Request(string ExchangeTypeTitle) : IRequest<Operation<LatestChangesOrderDto, string>>;

        public class Handler(IUnitOfWork UnitOfWork, IBus Bus)
            : IRequestHandler<Request, Operation<LatestChangesOrderDto, string>>
        {

            public async Task<Operation<LatestChangesOrderDto, string>> Handle(Request request, CancellationToken cancellationToken = default)
            {
                var exchangeEventRepository = UnitOfWork.GetRepository<ExchangeEvent>();

                var lastExchange = await exchangeEventRepository
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Enabled && x.Type == request.ExchangeTypeTitle,
                        orderBy: x => x.OrderByDescending(e => e.ExecutedAt));

                DateTime lastExchangeDate = default;

                if (lastExchange is not null)
                    lastExchangeDate = lastExchange.ExecutedAt;

                var currentExchangeEvent = ExchangeEvent.Create(request.ExchangeTypeTitle, DateTime.Now);

                if (!currentExchangeEvent.Ok)
                    return Operation.Error(currentExchangeEvent.Error);

                var result = new LatestChangesOrdersRequest(lastExchangeDate, currentExchangeEvent.Result.ExecutedAt);

                var response = await Bus.SendRequest<LatestChangesOrdersResponse>(
                    result,
                    timeout: TimeSpan.FromSeconds(15),
                    externalCancellationToken: cancellationToken);

                var orders = response.GetLatestChangesOrders();

                if (!orders.Ok)
                    return Operation.Error(orders.Error);

                var exportedOrders = orders.Result.Orders.Select(x => ExportedEntity.Create(currentExchangeEvent.Result, typeof(Order).Name, x.Code));

                await UnitOfWork.GetRepository<ExportedEntity>().InsertAsync(exportedOrders, cancellationToken);

                await exchangeEventRepository.InsertAsync(currentExchangeEvent.Result, cancellationToken);

                await UnitOfWork.SaveChangesAsync();

                if (UnitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(UnitOfWork.Result.Exception.Message);
                }

                await Bus.DeferLocal(TimeSpan.FromMinutes(5), new CheckOrderSyncCompletionCommand(currentExchangeEvent.Result.Id));

                orders.Result.Code = currentExchangeEvent.Result.Code;

                return orders;
            }
        }
    }
}
