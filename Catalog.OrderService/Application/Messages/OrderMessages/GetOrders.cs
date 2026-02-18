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
                Operation<List<OrderDto>, string> ordersResult;

                if (request.CacheKey is not null && request.Days <= Order.CacheDays)
                {
                    ordersResult = await cachedOrdersHandler
                        .HandleAsync(request.CacheKey, request.UserLogin, cancellationToken);
                }
                else
                {
                    ordersResult = await ordersHandler
                        .HandleAsync(
                            days: request.Days,
                            titlePattern: request.TitlePattern,
                            userLogin: request.UserLogin
                        );
                }

                if (!ordersResult.Ok)
                    return Operation.Error(ordersResult.Error);

                var orders = ordersResult.Result;

                if (orders.Any())
                {
                    if (request.Customers is not null && request.Customers.Any())
                    {
                        var usersSet = new HashSet<string>(request.Customers, StringComparer.OrdinalIgnoreCase);
                        orders = [.. orders.Where(x => usersSet.Contains(x.User))];
                    }

                    if (request.Statuses is not null && request.Statuses.Any())
                    {
                        var orderStatuses = request.Statuses.Select(x => (OrderStatus)Convert.ToInt16(x)).Select(x => x.ToRussianString());

                        var statusesSet = new HashSet<string>(orderStatuses, StringComparer.OrdinalIgnoreCase);

                        orders = [.. orders.Where(x => statusesSet.Contains(x.Status))];
                    }

                    orders = orders
                        .Where(x =>
                            x.CreatedAt > DateTime.Now.AddDays(-1 * request.Days)
                            && (!request.IncompleteOnly || !x.IsCompleted)
                            && (!request.CustomOnly || x.IsCustom)).ToList();

                    orders = request.Ascending ? [.. orders.OrderBy(x => x.CreatedAt)]
                        : [.. orders.OrderByDescending(x => x.CreatedAt)];
                }

                var pagedResult = PagedList
                    .Create(orders
                        .Where(x => string.IsNullOrWhiteSpace(request.TitlePattern) || x.Title.Contains(request.TitlePattern))
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