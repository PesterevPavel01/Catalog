using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using MediatR;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Messages.OrderMessages;

public class SendToProduction
{
    public record Request(IEnumerable<string> OrderCodes) : IRequest<Operation<bool, List<string>>>;

    public class Handler(IUnitOfWork unitOfWork, IBus bus)
        : IRequestHandler<Request, Operation<bool, List<string>>>
    {
        /// <summary>Handles a request</summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        public async Task<Operation<bool, List<string>>> Handle(Request request, CancellationToken cancellationToken)
        {
            var orderRepository = unitOfWork
                    .GetRepository<Order>();

            var orders = await orderRepository
                .GetAllAsync(
                    predicate: x => request.OrderCodes.Contains(x.Code),
                    trackingType: TrackingType.Tracking,
                    include: Order.IncludeRequiredField());

            List<String> errors = [];

            if (!orders.Any())
            {
                errors.Add("Order not found!");
                return Operation.Error(errors);
            }

            List<Order> updateOrders = [];

            foreach (var code in request.OrderCodes)
            {
                var order = orders.FirstOrDefault(x => x.Code == code);

                if (order is null)
                {
                    errors.Add($"{"OrderService".ToUpper()} Error: Order not found! Code: {code}");
                    continue;
                }

                var sendToProductionResult = order.SendToProduction();

                if (!sendToProductionResult.Ok)
                    errors.Add($"{"OrderService".ToUpper()}. OrderCode: {code}. Error: {sendToProductionResult.Error}!");
                else
                    updateOrders.Add(order);
            }

            if (updateOrders.Any())
            {
                orderRepository.Update(updateOrders);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    errors.Add($"{"OrderService".ToUpper()}. Error: {unitOfWork.Result.Exception.Message}!");
                }
                else
                {
                    foreach (var item in updateOrders)
                    {
                        // TODO: Рефакторинг
                        await bus.Publish(new UpdateOrderCacheCommand(item.Code));
                    }
                }
            }

            if(errors.Any())
                return Operation.Error(errors); 
            
            return true;
        }
    }
}