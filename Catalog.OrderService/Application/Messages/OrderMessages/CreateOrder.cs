using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Catalog.FacadeOrderTitleValidator;
using MediatR;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class CreateOrder
    {
        public record Request(CreateOrderDto Model) : IRequest<Operation<OrderDto, string>>;

        public class Handler(IUnitOfWork unitOfWork, ITitleValidator titleValidator)
            : IRequestHandler<Request, Operation<OrderDto, string>>
        {
            /// <summary>Handles a request</summary>
            /// <param name="request">The request</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Response from the request</returns>
            public async Task<Operation<OrderDto, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var user = await unitOfWork
                    .GetRepository<ApplicationUser>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.UserName == request.Model.UserName,
                        trackingType: TrackingType.Tracking);

                if (user is null)
                    return Operation.Error("User not found!");

                using var transaction = await unitOfWork.BeginTransactionAsync();

                var titleResult = await titleValidator.Validate(cancellationToken);

                if (!titleResult.Ok)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Operation.Error(titleResult.Error);
                }

                var orderEvent = OrderEvent.Create(OrderEventTypeTitles.Created, OrderEventType.Created, null);

                if (!orderEvent.Ok)
                    return Operation.Error(orderEvent.Error);

                var orderResult = Order
                    .Create(
                        title: titleResult.Result,
                        code: request.Model.OrderCode,
                        user,
                        orderEvent.Result);

                if (!orderResult.Ok)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Operation.Error(orderResult.Error);
                }

                /*await unitOfWork.GetRepository<OrderEvent>().InsertAsync(orderEvent.Result, cancellationToken); */
                await unitOfWork.GetRepository<Order>().InsertAsync(orderResult.Result, cancellationToken);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                await transaction.CommitAsync(cancellationToken);

                return orderResult.Result.ConvertToDto();
            }
        }
    }
}
