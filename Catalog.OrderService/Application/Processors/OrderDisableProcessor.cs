using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Domain.Entities;

namespace Catalog.OrderService.Application.Processors
{
    public class OrderDisableProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderDisableProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<bool, string>> ProcessAsync(String orderCode, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == orderCode,
                    trackingType: TrackingType.Tracking
                );

            if (order is null)
                return Operation.Error("Order not found!");

            order.Disable();

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return result > 0;
        }
    }
}
