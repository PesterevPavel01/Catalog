using Calabonga.UnitOfWork;
using Catalog.Contracts.Commands;
using Catalog.Domain.Entities;
using Rebus.Handlers;

namespace Catalog.OrderService.Application.QueueHandlers.OrderEventHandlers
{
    public class UpdateOrderCodeCommandHandler : IHandleMessages<UpdateOrderCodeCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderCodeCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(UpdateOrderCodeCommand message)
        {
            var order = await _unitOfWork
                .GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    trackingType: TrackingType.Tracking,
                    predicate: x => x.Code == message.Code);

            if(order is null)
                throw new InvalidOperationException("Order not found!");

            order.UpdateCode(message.NewCode);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                throw new ArgumentException($"{"OrderService".ToUpper()} Event {message.GetType().Name}. {_unitOfWork.Result.Exception.Message} OrderTitle: {order.Title}");
            }
        }
    }
}
