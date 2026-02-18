using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class DisableOrder
    {
        public record Request(string orderCode) : IRequest<Operation<OrderDto, string>>;

        public class Handler(IUnitOfWork unitOfWork)
            : IRequestHandler<Request, Operation<OrderDto, string>>
        {
            /// <summary>Handles a request</summary>
            /// <param name="request">The request</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Response from the request</returns>
            public async Task<Operation<OrderDto, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var order = await unitOfWork.GetRepository<Order>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == request.orderCode,
                        include: query => query.Include(x => x.ApplicationUser),
                        trackingType: TrackingType.Tracking
                    );

                if (order is null)
                    return Operation.Error("Order not found!");

                order.Disable();

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                return new OrderDto()
                {
                    Code = order.Code,
                    Title = order.Title.Value,
                    User = order.ApplicationUser.UserName,
                    IsApprovalCompleted = false
                };
            }
        }
    }
}
