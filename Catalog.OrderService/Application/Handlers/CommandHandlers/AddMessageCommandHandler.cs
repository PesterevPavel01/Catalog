using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Message;
using Catalog.Contracts.Entities;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;

namespace Catalog.OrderService.Application.Handlers.CommandHandlers
{
    public sealed class AddMessageCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddMessageCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<bool, string>> HandleAsync(CreateMessageDto model, CancellationToken cancellationToken) 
        {
            var orderItem = await _unitOfWork
                .GetRepository<OrderItem>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Order.Code == model.OrderCode && x.Module.Code == model.ModuleCode,
                    trackingType: TrackingType.Tracking);

            if (orderItem is null)
                return Operation.Error("OrderItem not found!");

            var user = await _unitOfWork
                .GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == model.SenderName,
                    trackingType: TrackingType.Tracking);

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

            return true;
        }
    }
}
