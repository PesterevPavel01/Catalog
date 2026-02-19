using Calabonga.OperationResults;
using Calabonga.PagedListCore;
using Catalog.Contracts.Dto;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Enum;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using MediatR;

namespace Catalog.OrderService.Application.Messages.OrderMessages
{
    public sealed class GetOrders
    {
        /// <summary>
        /// Handles a request
        /// </summary>
        /// <param name="Days"></param>
        /// <param name="Customers"></param>
        /// <param name="Statuses"></param>
        /// <param name="CacheKey"></param>
        /// <param name="TitlePattern"></param>
        /// <param name="UserLogin"></param>
        /// <param name="Ascending"></param>
        /// <param name="IncompleteOnly"></param>
        /// <param name="CustomOnly"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        public record Request(
            int Days,
            string[]? Customers = null,
            short[]? Statuses = null,
            string? CacheKey = null,
            string? TitlePattern = null,
            string? UserLogin = null,
            bool Ascending = false, 
            bool IncompleteOnly = false,
            bool CustomOnly = false,
            int PageIndex = 0, 
            int PageSize = 20)
            : IRequest<Operation<PagedResponseDto<CommonOrderDto>, string>>;

        public class Handler(OrdersQueryHandler ordersHandler, CachedOrdersQueryHandler cachedOrdersHandler)
            : IRequestHandler<Request, Operation<PagedResponseDto<CommonOrderDto>, string>>
        {
            public async Task<Operation<PagedResponseDto<CommonOrderDto>, string>> Handle(Request request, CancellationToken cancellationToken)
            {
                IQueryable<OrderDto> queriableOrder;

                if (request.CacheKey is not null && request.Days <= Order.CacheDays)
                {
                    var ordersResult = await cachedOrdersHandler
                        .HandleAsync(request.CacheKey, request.UserLogin, cancellationToken);

                    if (!ordersResult.Ok)
                        return Operation.Error(ordersResult.Error);

                    queriableOrder = ordersResult.Result.AsQueryable();

                    queriableOrder = queriableOrder
                        .Where(x => string.IsNullOrWhiteSpace(request.TitlePattern) || x.Title.Contains(request.TitlePattern));
                }
                else
                {
                    var ordersResult = await ordersHandler
                        .HandleAsync(
                            days: request.Days,
                            titlePattern: request.TitlePattern,
                            userLogin: request.UserLogin
                        );

                    if (!ordersResult.Ok)
                        return Operation.Error(ordersResult.Error);

                    queriableOrder = ordersResult.Result.AsQueryable();
                }

                var cutoffDate = DateTime.Now.AddDays(-request.Days);

                if (queriableOrder.Any())
                {
                    if (request.Customers is not null && request.Customers.Any())
                    {
                        var usersSet = new HashSet<string>(request.Customers, StringComparer.OrdinalIgnoreCase);
                        queriableOrder = queriableOrder.Where(x => usersSet.Contains(x.User));
                    }

                    if (request.Statuses is not null && request.Statuses.Any())
                    {
                        var orderStatuses = request.Statuses.Select(x => (OrderStatus)Convert.ToInt16(x)).Select(x => x.ToRussianString());

                        var statusesSet = new HashSet<string>(orderStatuses, StringComparer.OrdinalIgnoreCase);

                        queriableOrder = queriableOrder.Where(x => statusesSet.Contains(x.Status));
                    }

                    queriableOrder = queriableOrder
                        .Where(x =>
                            x.CreatedAt > cutoffDate
                            && (!request.IncompleteOnly || !x.IsCompleted)
                            && (!request.CustomOnly || x.IsCustom));

                    queriableOrder = request.Ascending ? queriableOrder.OrderBy(x => x.CreatedAt)
                        : queriableOrder.OrderByDescending(x => x.CreatedAt);
                }

                var pagedResult = PagedList
                    .Create(queriableOrder
                        .Select(x => new CommonOrderDto()
                        {
                            Code = x.Code,
                            Title = x.Title,
                            UserName = x.User,
                            IsApprovalCompleted = x.IsApprovalCompleted,
                            IsCompleted = x.IsCompleted,
                            IsCustom = x.IsCustom,
                            CreatedAt = x.CreatedAt,
                            UpdatedAt = x.UpdatedAt,
                            Status = x.Status,
                        }), request.PageIndex, request.PageSize, 0);

                return
                    new PagedResponseDto<CommonOrderDto>(
                        items: pagedResult.Items,
                        totalCount: pagedResult.TotalCount,
                        pageIndex: pagedResult.PageIndex,
                        pageSize: pagedResult.PageSize);
            }
        }
    }
}