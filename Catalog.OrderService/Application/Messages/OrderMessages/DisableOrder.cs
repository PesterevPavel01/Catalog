using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class DisableOrder
    {
        public record Request(string orderCode) : IRequest<Operation<OrderDto, string>>;

        public class Handler(IUnitOfWork unitOfWork, IBus bus)
            : IRequestHandler<Request, Operation<OrderDto, string>>
        {
            /// <summary>Handles a request</summary>
            /// <param name="request">The request</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Response from the request</returns>
            public async Task<Operation<OrderDto, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var orderRepository = unitOfWork.GetRepository<Order>();
                var order = await orderRepository
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == request.orderCode,
                        include: query => query.Include(x => x.ApplicationUser),
                        trackingType: TrackingType.Tracking
                    );

                if (order is null)
                    return Operation.Error("Order not found!");

                var disableResult = order.Disable();

                if(!disableResult.Ok)
                    return Operation.Error(disableResult.Error);

                orderRepository.Update(order);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                //TODO Рефакторинг
                await bus.Publish(new UpdateOrderCacheCommand(order.Code));

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
