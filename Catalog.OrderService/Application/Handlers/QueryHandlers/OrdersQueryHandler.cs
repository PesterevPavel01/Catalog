using System.Text.Json;
using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto;
using Catalog.Contracts.Entities.Approval;
using Catalog.Domain.Entities;

namespace Catalog.OrderService.Application.Handlers.QueryHandlers
{
    public sealed class OrdersQueryHandler
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrdersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<PagedResponseDto<Order>, string>> HandleAsync(int days, string? code = null, string? userLogin = null, bool ascending = false, bool incompleteOnly = false, bool customOnly = false, CancellationToken cancellationToken = default, int pageIndex = 0, int pageSize = 20)
        {
            var pagedResult = await _unitOfWork
                .GetRepository<Order>()
                .GetPagedListAsync(
                    predicate: x => 
                        (code == null || x.Code == code)
                        && (userLogin == null || x.ApplicationUser.UserName == userLogin)
                        && x.Enabled
                        && (days == -1 || x.CreatedAt > DateTime.Now.AddDays(-1 * days))
                        && (!customOnly || customOnly && x.OrderItems.Any(item => item.Module.Components.Any(c => c.ComponentTextParameters.Any(p => p.ParameterType.Code == Component.CustomParameterTypeCode))))
                        && (!incompleteOnly || incompleteOnly && x.OrderItems.Any(item => item.ApprovalWorkflow == null || item.ApprovalWorkflow.ApprovalWorkflowItems.OrderBy(wflow => wflow.Number).Last().ApprovalStage.Code != ApprovalWorkflow.CompletedStageCode)),
                    orderBy: orders => 
                        ascending ? orders.OrderBy(x => x.CreatedAt) : orders.OrderByDescending(x => x.CreatedAt),
                    include: Order.IncludeRequiredField(),
                    pageSize: pageSize,
                    pageIndex: pageIndex,
                    trackingType: TrackingType.NoTracking);

            /*var stringResult = JsonSerializer.Serialize(
                pagedResult.Items.Select(order => order.ConvertToDto()),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });*/

            return
                    new PagedResponseDto<Order>(
                        items: pagedResult.Items,
                        totalCount: pagedResult.TotalCount,
                        pageIndex: pagedResult.PageIndex,
                        pageSize: pagedResult.PageSize);
        }
    }
}