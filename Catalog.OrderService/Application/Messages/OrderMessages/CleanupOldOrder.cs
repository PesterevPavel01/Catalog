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

                var orders = await orderRepository
                    .GetAllAsync(
                        predicate: Order.GetCombinedCleanupPredicate(request.ArchiveStorageDays.CompletedOrdersDays, request.ArchiveStorageDays.InactiveOrdersDays, request.ArchiveStorageDays.DisabledOrdersDays),
                        trackingType: TrackingType.Tracking
                    );

                if (orders.Any())
                    orderRepository.Delete(orders);

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