using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Message;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using MediatR;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class AddMessage
    {
        public record Request(CreateMessageDto Model) : IRequest<Operation<OrderDto, string>>;

        public class Handler(IUnitOfWork unitOfWork)
            : IRequestHandler<Request, Operation<OrderDto, string>>
        {
            /// <summary>Handles a request</summary>
            /// <param name="request">The request</param>
            /// <param name="cancellationToken">Cancellation token</param>
            /// <returns>Response from the request</returns>
            public async Task<Operation<OrderDto, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var orderRepository = unitOfWork
                    .GetRepository<Order>();

                var order = await orderRepository
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == request.Model.OrderCode,
                        include: Order.IncludeRequiredField(),
                        trackingType: TrackingType.Tracking);

                if (order is null)
                    return Operation.Error("Order not found!");

                var orderItem = order.OrderItems.FirstOrDefault(x => x.Module.Code == request.Model.ModuleCode);

                if (orderItem is null)
                    return Operation.Error("OrderItem not found!");

                var user = await unitOfWork
                    .GetRepository<ApplicationUser>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.UserName == request.Model.SenderName,
                        trackingType: TrackingType.Tracking);

                if (user is null)
                    return Operation.Error("ApplicationUser not found!");

                var message = Message.Create(request.Model.Text, orderItem, user);

                if (!message.Ok)
                    return Operation.Error(message.Error);

                var addResult = order.AddMessageToOrderItem(orderItem, message.Result);

                if (!addResult.Ok)
                    return Operation.Error(addResult.Error);

                orderRepository.Update(order);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                return order.ConvertToDto();
            }
        }
    }
}
