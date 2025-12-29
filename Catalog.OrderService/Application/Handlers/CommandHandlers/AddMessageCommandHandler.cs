using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Message;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Domain.Entities;

namespace Catalog.OrderService.Application.Handlers.CommandHandlers
{
    public sealed class AddMessageCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddMessageCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<OrderDto, string>> HandleAsync(CreateMessageDto model, CancellationToken cancellationToken) 
        {
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.OrderCode,
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

            if (order is null)
                return Operation.Error("Order not found!");

            var orderItem = order.OrderItems.FirstOrDefault(x => x.Module.Code == model.ModuleCode);

            if (orderItem is null)
                return Operation.Error("OrderItem not found!");

            var user = order.ApplicationUser;

            if (user is null)
                return Operation.Error("ApplicationUser not found!");

            var message = Message.Create(model.Text, orderItem, user);

            if (!message.Ok)
                return Operation.Error(message.Error);

            await _unitOfWork
                .GetRepository<Message>().InsertAsync(message.Result, cancellationToken);

            orderItem.AddMessage(message.Result);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return order.ConvertToDto();
        }
    }
}
