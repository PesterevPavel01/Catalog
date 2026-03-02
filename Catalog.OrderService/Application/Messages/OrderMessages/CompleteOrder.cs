using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using MediatR;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class CompleteOrder
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="OrderCodes"></param>
        public record Request(IEnumerable<String> OrderCodes) : IRequest<Operation<bool, string>>;
        public class Handler(IUnitOfWork unitOfWork, IBus bus)
            : IRequestHandler<Request, Operation<bool, string>>
        {
            public async Task<Operation<bool, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                if (request.OrderCodes is null || !request.OrderCodes.Any())
                    return Operation.Error("Order codes not found!");

                var orders = await unitOfWork
                    .GetRepository<Order>()
                    .GetAllAsync(
                        predicate: x => request.OrderCodes.Contains(x.Code),
                        include: Order.IncludeRequiredField());

                if (!orders.Any())
                    return Operation.Error("Order not found!");

                foreach (var code in request.OrderCodes)
                {
                    var order = orders.FirstOrDefault(x => x.Code == code);

                    if (order is null)
                        return Operation.Error("Order codes not found!");

                    await bus.Publish(new UpdateOrderCacheCommand(code));

                    await bus.Publish(new OrderCompletedEvent(order.ConvertToDto()));
                }

                return true;
            }
        }
    }
}
