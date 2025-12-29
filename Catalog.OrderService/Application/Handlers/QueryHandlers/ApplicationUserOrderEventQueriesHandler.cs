using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto;
using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Entities;

namespace Catalog.OrderService.Application.Handlers.QueryHandlers
{
    public class ApplicationUserOrderEventQueriesHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApplicationUserOrderEventQueriesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<PagedResponseDto<OrderEventDto>, string>> HandleAsync(string userName, CancellationToken cancellationToken = default, int pageIndex = 0, int pageSize = 20)
        {
            if (String.IsNullOrWhiteSpace(userName))
                return Operation.Error("Incorrect user name value!");

            var pagedResult = await _unitOfWork
                .GetRepository<OrderEvent>()
                .GetPagedListAsync(
                    predicate: x =>
                        x.Order.ApplicationUser.UserName == userName,
                    //    && x.Order.Enabled,                        
                    orderBy: x => x.OrderByDescending(e => e.CreatedAt),
                    include: OrderEvent.IncludeRequiredField(),
                    selector: x => x.ConvertToDto(),
                    pageSize: pageSize,
                    pageIndex: pageIndex,
                    trackingType: TrackingType.NoTracking);

            return
                new PagedResponseDto<OrderEventDto>(
                    items: pagedResult.Items,
                    totalCount: pagedResult.TotalCount,
                    pageIndex: pagedResult.PageIndex,
                    pageSize: pagedResult.PageSize);
        }
    }
}
