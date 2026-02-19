using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto;
using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Entities;
using MediatR;

namespace Catalog.OrderService.Application.Messages.OrderEventMessages
{
    public sealed class GetOrderEvents
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        public record Request( int PageIndex = 0, int PageSize = 20)
            : IRequest<Operation<PagedResponseDto<OrderEventDto>, string>>;

        public class Handler(IUnitOfWork unitOfWork)
            : IRequestHandler<Request, Operation<PagedResponseDto<OrderEventDto>, string>>
        {
            public async Task<Operation<PagedResponseDto<OrderEventDto>, string>> Handle(Request request, CancellationToken cancellationToken = default)
            {
                var pagedResult = await unitOfWork
                    .GetRepository<OrderEvent>()
                    .GetPagedListAsync(
                        //predicate: x => x.Order.Enabled,
                        orderBy: x => x.OrderByDescending(e => e.CreatedAt),
                        include: OrderEvent.IncludeRequiredField(),
                        selector: x => x.ConvertToDto(),
                        pageSize: request.PageSize,
                        pageIndex: request.PageIndex,
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
}
