using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Domain.Entities;

namespace Catalog.OrderService.Application.Processors
{
    public class OrderItemRemovalProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderItemRemovalProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<bool, string>> ProcessAsync(string orderCode, string moduleCode, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.GetRepository<Order>()
            .GetFirstOrDefaultAsync(
                predicate: x => x.Code == orderCode,
                include: Order.IncludeRequiredField(),
                trackingType: TrackingType.Tracking
            );

            if (order is null)
                return Operation.Error("Order not found!");

            if (order.IsApprovalCompleted())
                return Operation.Error("Order is completed!");

            var orderItem = await _unitOfWork.GetRepository<OrderItem>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Order.Code == orderCode && x.Module.Code == moduleCode,
                    trackingType: TrackingType.Tracking
                );

            if (order.OrderItems.FirstOrDefault(x => x.Module.Code == moduleCode) is null)
                return Operation.Error("OrderItem not found!");

            order.RemoveOrderItem(orderItem);

            _unitOfWork.GetRepository<OrderItem>().Delete(orderItem);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return result > 0;
        }
    }
}
