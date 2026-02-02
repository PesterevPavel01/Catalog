using Calabonga.OperationResults;
using Calabonga.PagedListCore;
using Catalog.Contracts.Dto;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Enum;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Handlers.QueryHandlers;

namespace Catalog.OrderService.Application.Managers
{
    public class OrderQueriesManager
    {
        private readonly OrdersQueryHandler _ordersHandler;
        private readonly CachedOrdersQueryHandler _cachedOrdersHandler;

        public OrderQueriesManager(OrdersQueryHandler ordersHandler, CachedOrdersQueryHandler cachedOrdersHandler)
        {
            _ordersHandler = ordersHandler;
            _cachedOrdersHandler = cachedOrdersHandler;
        }

        public async Task<Operation<PagedResponseDto<CommonOrderDto>, string>> HandleAsync(int days, string[]? customers = null, short[]? statuses = null, string? cacheKeyType = null, string? titlePattern = null, string? userLogin = null, bool ascending = false, bool incompleteOnly = false, bool customOnly = false, CancellationToken cancellationToken = default, int pageIndex = 0, int pageSize = 20)
        {
            Operation<List<OrderDto>,string> ordersResult;

            if (cacheKeyType is not null && days <= Order.CacheDays)
            {
                ordersResult = await _cachedOrdersHandler
                    .HandleAsync(cacheKeyType, userLogin, cancellationToken);
            }
            else
            {
                ordersResult = await _ordersHandler
                    .HandleAsync(
                        days: days, 
                        titlePattern: titlePattern,
                        userLogin: userLogin
                    );
            }

            if (!ordersResult.Ok)
                return Operation.Error(ordersResult.Error);

            var orders = ordersResult.Result;

            if (orders.Any())
            {
                if (customers is not null && customers.Any())
                {
                    var usersSet = new HashSet<string>(customers, StringComparer.OrdinalIgnoreCase);
                    orders = orders.Where(x => usersSet.Contains(x.User)).ToList();
                }

                if (statuses is not null && statuses.Any())
                {
                    var orderStatuses = statuses.Select(x => (OrderStatus)Convert.ToInt16(x)).Select(x => x.ToRussianString());

                    var statusesSet = new HashSet<string>(orderStatuses, StringComparer.OrdinalIgnoreCase);

                    orders = orders.Where(x => statusesSet.Contains(x.Status)).ToList();
                }

                orders = orders
                    .Where(x =>
                        (x.CreatedAt > DateTime.Now.AddDays(-1 * days))
                        && (!incompleteOnly || !x.IsCompleted)
                        && (!customOnly || x.IsCustom)).ToList();

                orders = ascending ? orders
                    .OrderBy(x => x.CreatedAt).ToList()
                    : orders
                    .OrderByDescending(x => x.CreatedAt).ToList();
            }

            var pagedResult = PagedList
                .Create(orders
                    .Where(x => string.IsNullOrWhiteSpace(titlePattern) || x.Title.Contains(titlePattern))
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
                    }), pageIndex, pageSize, 0);

            return
                new PagedResponseDto<CommonOrderDto>(
                    items: pagedResult.Items,
                    totalCount: pagedResult.TotalCount,
                    pageIndex: pagedResult.PageIndex,
                    pageSize: pagedResult.PageSize);
        }
    }
}