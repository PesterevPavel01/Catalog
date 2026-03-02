using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using MediatR;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Messages.OrderMessages;

public class CompleteProduction 
{ 
    /// <summary>
    /// Handles a request
    /// </summary>
    /// <param name="OrderCodes"></param>
    public record Request(IEnumerable<String> OrderCodes) : IRequest<Operation<bool, List<string>>>;
    public class Handler(IUnitOfWork unitOfWork, IBus bus)
        : IRequestHandler<Request, Operation<bool, List<string>>>
    {
        public async Task<Operation<bool, List<string>>> Handle(Request request, CancellationToken cancellationToken)
        {
            List<String> errors = [];

            if (request.OrderCodes is null || !request.OrderCodes.Any()) {
                errors.Add("Order codes not found!");
                return Operation.Error(errors);
            }

            var orderRepository = unitOfWork.GetRepository<Order>();

            var orders = await orderRepository
                .GetAllAsync(
                    predicate: x => request.OrderCodes.Contains(x.Code),
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

            if (!orders.Any()) {
                errors.Add("Order not found!");
                return Operation.Error(errors);
            }

            foreach (var code in request.OrderCodes)
            {
                var order = orders.FirstOrDefault(x => x.Code == code);

                if (order is null) {
                    errors.Add("Order codes not found!");
                    continue;
                }

                var completeProductionResult = order.CompleteProduction();

                if(!completeProductionResult.Ok) {
                    errors.Add(completeProductionResult.Error);
                    continue;
                }
            }

            orderRepository.Update(orders);

            var result = await unitOfWork.SaveChangesAsync();

            if (unitOfWork.Result.Exception is not null)
                errors.Add(unitOfWork.Result.Exception.Message);

            if(errors.Any())
                return Operation.Error(errors);

            foreach (var order in orders) 
            {
                await bus.Publish(new UpdateOrderCacheCommand(order.Code));
            }

            return true;
        }
    }

}
