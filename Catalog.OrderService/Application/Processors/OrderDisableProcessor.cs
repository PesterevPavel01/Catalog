using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.OrderService.Application.Processors
{
    public class OrderDisableProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderDisableProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<OrderDto, string>> ProcessAsync(String orderCode, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == orderCode,
                    include: query => query.Include(x => x.ApplicationUser),
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

            return new OrderDto() 
            {
                Code = order.Code,
                Title = order.Title.Value,
                User = order.ApplicationUser.UserName,
                IsApprovalCompleted = false
            };
        }
    }
}
