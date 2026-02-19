using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.OrderService.Application.Messages.OrderItemMessages
{
    public sealed class RemoveOrderItem
    {
        public record Request(string OrderCode, string ModuleCode) : IRequest<Operation<bool, string>>;

        public class Handler(IUnitOfWork unitOfWork)
            : IRequestHandler<Request, Operation<bool, string>>
        {

            public async Task<Operation<bool, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var order = await unitOfWork.GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == request.OrderCode,
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking
                );

                if (order is null)
                    return Operation.Error("Order not found!");

                if (order.IsApprovalCompleted())
                    return Operation.Error("Order is completed!");

                var orderItem = await unitOfWork.GetRepository<OrderItem>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Order.Code == request.OrderCode && x.Module.Code == request.ModuleCode,
                        trackingType: TrackingType.Tracking
                    );

                if (order.OrderItems.FirstOrDefault(x => x.Module.Code == request.ModuleCode) is null)
                    return Operation.Error("OrderItem not found!");

                order.RemoveOrderItem(orderItem);

                unitOfWork.GetRepository<OrderItem>().Delete(orderItem);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                return result > 0;
            }
        }
    }
}
