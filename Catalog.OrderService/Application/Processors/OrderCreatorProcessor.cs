using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Autorization;

namespace Catalog.OrderService.Application.Processors
{
    public sealed class OrderCreatorProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderCreatorProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<OrderDto, string>> ProcessAsync(CreateOrderDto model, CancellationToken cancellationToken) 
        {

            var modelModules = model.OrderItems.Select(model => model.ModuleCode).ToList();

            var modules = await _unitOfWork
                .GetRepository<Module>().GetAllAsync(
                    predicate: entity => modelModules.Contains(entity.Code),
                    trackingType: TrackingType.Tracking,
                    include: Module.IncludeRequaredField());

            List<OrderItem> orderItems = [];

            foreach (var x in model.OrderItems)
            {
                var orderItemResult = OrderItem.Create(x.Quantity, modules.FirstOrDefault(module => module.Code == x.ModuleCode));
                
                if(!orderItemResult.Ok)
                    return Operation.Error(orderItemResult.Error);

                orderItems.Add(orderItemResult.Result);
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.GetRepository<OrderItem>().InsertAsync(orderItems, cancellationToken);

            var user = await _unitOfWork
                .GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == model.UserName,
                    trackingType: TrackingType.Tracking);

            if (user is null)
                return Operation.Error("User not found!");

            var orderResult = Order.Create(title: model.OrderTitle, code: model.OrderCode, user, orderItems);

            if (!orderResult.Ok)
                return Operation.Error(orderResult.Error);

            await _unitOfWork.GetRepository<Order>().InsertAsync(orderResult.Result, cancellationToken);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            await transaction.CommitAsync(cancellationToken);

            return orderResult.Result.ConvertToDto();
        }
    }
}
