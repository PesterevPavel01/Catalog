using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using MediatR;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Messages.OrderItemMessages;

public sealed class UpdateModule
{
    public record Request(String ModuleCode) : IRequest<Operation<bool, List<string>>>;

    public class Handler(IUnitOfWork unitOfWork, IBus bus)
        : IRequestHandler<Request, Operation<bool, List<string>>>
    {
        public async Task<Operation<bool, List<string>>> Handle(Request request, CancellationToken cancellationToken)
        {
            List<String> errors = [];

            var orderRepository = unitOfWork.GetRepository<Order>();

            var orders = await orderRepository
                .GetAllAsync(
                    predicate: x => x.OrderItems.Any(item => item.Module.Code == request.ModuleCode),
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking
                );

            if (!orders.Any())
            {
                errors.Add($"{"OrderService".ToUpper()} Event {request.GetType().Name}. Order not found! ModuleCode: {request.ModuleCode}");
                return Operation.Error(errors);
            }

            foreach (var order in orders)
            {

                var changeResult = order.ModuleChange();

                if (!changeResult.Ok)
                {
                    errors.Add($"{"OrderService".ToUpper()} Event {request.GetType().Name}. Error: {changeResult.Error}. OrderCode: {order.Code}");
                    continue;
                }

                orderRepository.Update(order);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    errors.Add($"{"OrderService".ToUpper()} Event {request.GetType().Name}. Error: {unitOfWork.Result.Exception.Message}. OrderCode: {order.Code}");
                    continue;
                }

                await bus.Publish(new UpdateOrderCacheCommand(order.Code));
            }

            if(errors.Any())
                return Operation.Error(errors);

            return true;
        }
    }
}
