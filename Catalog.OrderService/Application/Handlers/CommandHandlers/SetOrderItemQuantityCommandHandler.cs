using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;

namespace Catalog.OrderService.Application.Handlers.CommandHandlers
{
    public sealed class SetOrderItemQuantityCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public SetOrderItemQuantityCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<bool, string>> HandleAsync(CreateOrderItemDto model, CancellationToken cancellationToken) 
        {
            var orderItem = await _unitOfWork
                .GetRepository<OrderItem>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Order.Code == model.OrderCode && x.Module.Code == model.ModuleCode,
                    trackingType: TrackingType.Tracking);

            if (orderItem is null)
                return Operation.Error("OrderItem not found!");

            if(orderItem.Quantity == model.Quantity)
                return true;

            orderItem.SetQuantity(model.Quantity);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return true;
        }
    }
}
