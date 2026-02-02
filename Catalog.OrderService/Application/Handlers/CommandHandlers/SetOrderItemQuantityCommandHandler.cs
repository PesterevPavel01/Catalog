using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Rebus.Bus;
using Microsoft.EntityFrameworkCore;

namespace Catalog.OrderService.Application.Handlers.CommandHandlers
{
    public sealed class SetOrderItemQuantityCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;

        public SetOrderItemQuantityCommandHandler(IUnitOfWork unitOfWork, IBus bus)
        {
            _unitOfWork = unitOfWork;
            _bus = bus;
        }

        public async Task<Operation<bool, string>> HandleAsync(CreateOrderItemDto model, CancellationToken cancellationToken) 
        {
            var orderItem = await _unitOfWork
                .GetRepository<OrderItem>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Order.Code == model.OrderCode && x.Module.Code == model.ModuleCode,
                    include: query => query
                        .Include(x => x.Order),
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

            await _bus.Send(new OrderChangedEvent(orderItem.Order.Code));

            return true;
        }
    }
}
