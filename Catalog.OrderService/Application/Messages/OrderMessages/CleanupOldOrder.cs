using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Configurations;
using Catalog.Domain.Entities;
using MediatR;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class CleanupOldOrder
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="ArchiveStorageDays"></param>
        public record Request(OrderCleanupSettings ArchiveStorageDays) : IRequest<Operation<bool, string>>;

        public class Handler(IUnitOfWork unitOfWork)
            : IRequestHandler<Request, Operation<bool, string>>
        {

            public async Task<Operation<bool, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                var orderRepository = unitOfWork.GetRepository<Order>();

                /*Completed orders*/
                var completedOrders = await orderRepository
                    .GetAllAsync(
                        predicate: Order.IsCompletedBefore(request.ArchiveStorageDays.CompletedOrdersDays),
                        trackingType: TrackingType.Tracking
                    );

                if(completedOrders.Any())
                    orderRepository.Delete(completedOrders);

                /*Inactive orders*/
                var inactiveOrders = await orderRepository
                    .GetAllAsync(
                        predicate: Order.IsInactiveBefore(request.ArchiveStorageDays.InactiveOrdersDays),
                        trackingType: TrackingType.Tracking
                    );

                if (inactiveOrders.Any())
                    orderRepository.Delete(inactiveOrders);

                /*Disabled orders*/
                var disabledOrders = await orderRepository
                    .GetAllAsync(
                        predicate: x => !x.Enabled && x.UpdatedAt < DateTime.Now.AddDays(request.ArchiveStorageDays.DisabledOrdersDays * -1),
                        trackingType: TrackingType.Tracking);

                if (disabledOrders.Any())
                    unitOfWork.GetRepository<Order>().Delete(disabledOrders);

                var result = await unitOfWork.SaveChangesAsync();

                if (unitOfWork.Result.Exception is not null)
                {
                    return Operation.Error(unitOfWork.Result.Exception.Message);
                }

                return true;
            }
        }
    }
}