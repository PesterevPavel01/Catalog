using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Enum;
using Catalog.Domain.Entities;

namespace Catalog.OrderService.Application.Handlers.QueryHandlers
{
    public sealed class LastModifiedOrdersQueryHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public LastModifiedOrdersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<LatestChangesOrderDto, string>> HandleAsync(DateTime dateStart, DateTime dateEnd, CancellationToken cancellationToken = default)
        {
            var createdOrders = await _unitOfWork
                .GetRepository<Order>()
                .GetAllAsync(
                    predicate: x =>
                        x.Enabled 
                        && x.OrderHistory.Where(x => x.CreatedAt > dateStart && x.CreatedAt <= dateEnd).Any()
                        && x.OrderHistory.Where(x => x.CreatedAt > dateStart && x.CreatedAt <= dateEnd).OrderByDescending(x => x.CreatedAt).FirstOrDefault().Type == (Int32)OrderEventType.ApprovalCompleted,
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.NoTracking);

            return
                new LatestChangesOrderDto()
                { 
                    Orders = createdOrders.Select(x => x.ConvertToDto().SetUser(x.ApplicationUser.ExternalId)),
                };
        }
    }
}