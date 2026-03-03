using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.Approval;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using MediatR;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Messages.OrderItemMessages
{
    public sealed class RemoveOrderItem
    {
        public record Request(string OrderCode, string ModuleCode) : IRequest<Operation<bool, string>>;

        public class Handler(IUnitOfWork unitOfWork, IBus bus)
            : IRequestHandler<Request, Operation<bool, string>>
        {

            public async Task<Operation<bool, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var orderRepository = unitOfWork.GetRepository<Order>();

                var order = await orderRepository
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == request.OrderCode,
                        include: Order.IncludeRequiredField(),
                        trackingType: TrackingType.Tracking
                    );

                if (order is null)
                    return Operation.Error("Order not found!");

                var orderItem = await unitOfWork.GetRepository<OrderItem>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Order.Code == request.OrderCode && x.Module.Code == request.ModuleCode,
                        trackingType: TrackingType.Tracking
                    );

                if (order.OrderItems.FirstOrDefault(x => x.Module.Code == request.ModuleCode) is null)
                    return Operation.Error("OrderItem not found!");

                var removeResult = order.RemoveOrderItem(orderItem);

                if(!removeResult.Ok)
                    return Operation.Error(removeResult.Error);

                orderRepository.Update(order);

                unitOfWork.GetRepository<OrderItem>().Delete(orderItem);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                await bus.Publish(new UpdateOrderCacheCommand(order.Code));

                return result > 0;
            }
        }
    }
}
