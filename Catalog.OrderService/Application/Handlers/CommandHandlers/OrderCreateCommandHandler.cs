using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Catalog.FacadeOrderTitleValidator;

namespace Catalog.OrderService.Application.Handlers.CommandHandlers
{
    public sealed class OrderCreateCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITitleValidator _titleValidator;

        public OrderCreateCommandHandler(IUnitOfWork unitOfWork, ITitleValidator titleValidator)
        {
            _unitOfWork = unitOfWork;
            _titleValidator = titleValidator;
        }

        public async Task<Operation<OrderDto, string>> HandleAsync(CreateOrderDto model, CancellationToken cancellationToken) 
        {
            var user = await _unitOfWork
                .GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == model.UserName,
                    trackingType: TrackingType.Tracking);

            if (user is null)
                return Operation.Error("User not found!");

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var titleResult = await _titleValidator.Validate(cancellationToken);

            if (!titleResult.Ok)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Operation.Error(titleResult.Error);
            }

            var orderResult = Order
                .Create(
                    title: titleResult.Result, 
                    code: model.OrderCode, user);

            if (!orderResult.Ok)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Operation.Error(orderResult.Error);
            }

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
