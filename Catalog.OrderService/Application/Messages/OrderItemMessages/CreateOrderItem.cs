using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using MediatR;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Messages.OrderItemMessages
{
    public class CreateOrderItem
    {
        public record Request(IEnumerable<CreateOrderItemDto> models) : IRequest<Operation<OrderDto, string>>;

        public class Handler(IUnitOfWork unitOfWork, IBus bus,
            IOrderValidator orderValidator, IOrderExtendabilityValidator extendabilityValidator)
            : IRequestHandler<Request, Operation<OrderDto, string>>
        {

            /// <summary>Handles a request</summary>
            /// <param name="request">The request</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Response from the request</returns>

            public async Task<Operation<OrderDto, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var orderCode = request.models.First(x => x.OrderCode != null).OrderCode;

                var orderRepository = unitOfWork.GetRepository<Order>();

                var order = await orderRepository
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == orderCode,
                        trackingType: TrackingType.Tracking,
                        include: Order.IncludeRequiredField()
                    );

                if (order is null)
                    return Operation.Error("Order not found!");

                var orderItemsModels = request.models
                    .GroupBy(x => x.ModuleCode)
                    .Select(group => new CreateOrderItemDto()
                    {
                        ModuleCode = group.Key,
                        Quantity = (short)group.Sum(item => item.Quantity)
                    });

                var modules = await unitOfWork.GetRepository<Module>()
                .GetAllAsync(
                    predicate: x => orderItemsModels.Select(m => m.ModuleCode).Contains(x.Code) && x.Enabled,
                    trackingType: TrackingType.Tracking,
                    include: Module.IncludeRequiredField()
                );

                if (!modules.Any())
                    return Operation.Error("Modules not found!");

                IEnumerable<OrderItem> newOrderItems = [];

                var existingOrderItems = order.OrderItems.Where(x => modules.Select(m => m.Code).Contains(x.Module.Code) && x.Enabled);

                if (existingOrderItems.Any())
                {
                    foreach (var item in existingOrderItems)
                    {
                        order.ChangeItemQuantity(item, (short)(item.Quantity + orderItemsModels.First(x => x.ModuleCode == item.Module.Code).Quantity));
                    }
                }
                else
                {
                    var newOrderItemModules = modules.Where(x => !existingOrderItems.Select(x => x.Module.Code).Contains(x.Code));

                    if (newOrderItemModules.Any())
                    {
                        var orderItemResult = newOrderItemModules
                            .Select(x =>
                                OrderItem.Create(
                                    orderItemsModels.First(o => o.ModuleCode == x.Code).Quantity,
                                    x));

                        if (orderItemResult.FirstOrDefault(x => !x.Ok))
                            return Operation.Error(orderItemResult.FirstOrDefault(x => !x.Ok).Error);

                        newOrderItems = orderItemResult.Select(x => x.Result);

                        foreach (var item in newOrderItems)
                        {
                            var operationResult = order.AddOrderItem(item, orderValidator, extendabilityValidator);

                            if (!operationResult.Ok)
                                return Operation.Error(operationResult.Error);
                        }
                    }
                }

                using var transaction = await unitOfWork.BeginTransactionAsync();

                orderRepository.Update(order);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                await transaction.CommitAsync(cancellationToken);

                //TODO Рефакторинг
                await bus.Publish(new UpdateOrderCacheCommand(order.Code));

                return order.ConvertToDto();

            }
        }
    }
}
