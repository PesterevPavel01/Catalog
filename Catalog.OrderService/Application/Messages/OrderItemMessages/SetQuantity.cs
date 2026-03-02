using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using MediatR;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Messages.OrderItemMessages
{
    public sealed class SetQuantity
    {
        public record Request(CreateOrderItemDto model) : IRequest<Operation<bool, string>>;

        public class Handler(IUnitOfWork unitOfWork, IBus bus)
            : IRequestHandler<Request, Operation<bool, string>>
        {
            public async Task<Operation<bool, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var orderRepository = unitOfWork
                    .GetRepository<Order>();

                var order = await orderRepository
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == request.model.OrderCode,
                        include: Order.IncludeRequiredField(),
                        trackingType: TrackingType.Tracking);

                if (order is null)
                    return Operation.Error("Order not found!");

                var orderItem = order.OrderItems.FirstOrDefault(x => x.Module.Code == request.model.ModuleCode);

                if (orderItem is null)
                    return Operation.Error("OrderItem not found!");

                var changeQuantityResult = order.ChangeItemQuantity(orderItem, request.model.Quantity);

                if(!changeQuantityResult.Ok)
                    return Operation.Error(changeQuantityResult.Error);

                orderRepository.Update(order);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                //TODO Рефакторинг
                await bus.Publish(new UpdateOrderCacheCommand(order.Code));

                return true;
            }
        }
    }
}
