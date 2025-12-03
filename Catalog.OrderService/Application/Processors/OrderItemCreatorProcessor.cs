using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;

namespace Catalog.OrderService.Application.Processors
{
    public class OrderItemCreatorProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderItemCreatorProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<List<Guid>, string>> ProcessAsync(IEnumerable<CreateOrderItemDto> models, CancellationToken cancellationToken)
        {
            var orderCode = models.First(x => x.OrderCode != null).OrderCode;

            var order = await _unitOfWork.GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == orderCode,
                    trackingType: TrackingType.Tracking,
                    include: Order.IncludeRequiredField()
                );

            if (order is null)
                return Operation.Error("Order not found!");

            if(order.IsCompleted())
                return Operation.Error("Order is completed!");

            if (order.OrderItems.Where(x => x.ApprovalWorkflow is not null).Any())
                return Operation.Error("У заказа запущен процесс согласования!");

            var orderItemsModels = models
                .GroupBy(x => x.ModuleCode)
                .Select(group => new CreateOrderItemDto()
                {
                    ModuleCode = group.Key,
                    Quantity = (short)group.Sum(item => item.Quantity)
                });

            var modules = await _unitOfWork.GetRepository<Module>()
            .GetAllAsync(
                predicate: x => orderItemsModels.Select(m => m.ModuleCode).Contains(x.Code) && x.Enabled,
                trackingType: TrackingType.Tracking
            );

            if (!modules.Any())
                return Operation.Error("Modules not found!");

            IEnumerable<OrderItem> newOrderItems = [];

            var existingOrderItems = order.OrderItems.Where(x => modules.Select( m => m.Code).Contains(x.Module.Code) && x.Enabled);

            if (existingOrderItems.Any())
            {
                foreach (var item in existingOrderItems)
                {
                    item.SetQuantity((short)(item.Quantity + orderItemsModels.First(x => x.ModuleCode == item.Module.Code).Quantity));
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
                        var operationResult = order.AddOrderItem(item);

                        if (!operationResult.Ok)
                            return Operation.Error(operationResult.Error);
                    }  
                }
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.GetRepository<OrderItem>().InsertAsync(newOrderItems, cancellationToken);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            await transaction.CommitAsync(cancellationToken);

            List<Guid> newOrderItemsId = [.. newOrderItems.Select(x => x.Id)];

            return newOrderItemsId;

        }
    }
}
