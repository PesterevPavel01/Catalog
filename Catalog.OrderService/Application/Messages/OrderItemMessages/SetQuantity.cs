using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.OrderService.Application.Messages.OrderItemMessages
{
    public sealed class SetQuantity
    {
        public record Request(CreateOrderItemDto model) : IRequest<Operation<bool, string>>;

        public class Handler(IUnitOfWork unitOfWork)
            : IRequestHandler<Request, Operation<bool, string>>
        {
            public async Task<Operation<bool, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var orderItem = await unitOfWork
                    .GetRepository<OrderItem>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Order.Code == request.model.OrderCode && x.Module.Code == request.model.ModuleCode,
                        include: query => query
                            .Include(x => x.Order),
                        trackingType: TrackingType.Tracking);

                if (orderItem is null)
                    return Operation.Error("OrderItem not found!");

                if (orderItem.Quantity == request.model.Quantity)
                    return true;

                orderItem.SetQuantity(request.model.Quantity);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                return true;
            }
        }
    }
}
