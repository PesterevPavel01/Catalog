using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Order;
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

        public async Task<Operation<List<OrderDto>, string>> HandleAsync(int days, string? cacheKeyType = null, string? titlePattern = null, string? code = null, string? userLogin = null, bool ascending = false, bool incompleteOnly = false, bool customOnly = false, CancellationToken cancellationToken = default, int pageIndex = 0, int pageSize = 20)
        {
            var orders = await _unitOfWork
                .GetRepository<Order>()
                .GetAllAsync(
                    predicate: x =>
                        (code == null || x.Code == code)
                        && (userLogin == null || x.ApplicationUser.UserName == userLogin)
                        && x.Enabled
                        && (days == -1 || x.CreatedAt > DateTime.Now.AddDays(-1 * days))
                        && (!customOnly || customOnly && x.OrderItems.Any(item => item.Module.Components.Any(c => c.ComponentTextParameters.Any(p => p.ParameterType.Code == Component.CustomParameterTypeCode))))
                        && (!incompleteOnly || incompleteOnly && x.OrderItems.Any(item => item.ApprovalWorkflow == null || item.ApprovalWorkflow.ApprovalWorkflowItems.OrderBy(wflow => wflow.Number).Last().ApprovalStage.Code != ApprovalWorkflow.CompletedStageCode)),
                    include: Order.IncludeRequiredField(),
                    selector: x => x.ConvertToDto(),
                    trackingType: TrackingType.NoTracking);

            if (!string.IsNullOrWhiteSpace(titlePattern))
                orders = orders.Where(x => x.Title.Contains(titlePattern)).ToList();

            return orders.ToList();
        }

        public async Task<Operation<List<Order>, string>> HandleAsync(string? code = null, CancellationToken cancellationToken = default, int pageIndex = 0, int pageSize = 20)
        {
            var orders = await _unitOfWork
                .GetRepository<Order>()
                .GetAllAsync(
                    predicate: x =>
                        (code == null || x.Code == code)
                        && x.Enabled,
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.NoTracking);

            return orders.ToList();
        }
    }
}