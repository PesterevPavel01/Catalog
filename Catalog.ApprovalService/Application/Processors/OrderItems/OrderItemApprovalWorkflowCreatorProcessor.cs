using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Application.Processors.OrderItems
{
    public class OrderItemApprovalWorkflowCreatorProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<ApprovalWorkflowMapItem> _workflowMap;

        public OrderItemApprovalWorkflowCreatorProcessor( IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _workflowMap = applicationConfiguration.Value.ApprovalWorkflowMap;
        }

        /// <summary>
        /// starts ApprovalWorkflow only for OrderItems that are in the input model
        /// </summary>
        /// <param name="models"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        public async Task<Operation<Order, string>> ProcessAsync(OrderDto model, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.GetRepository<Order>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.Code && x.Enabled,
                    trackingType: TrackingType.Tracking,
                    include: Order.IncludeRequiredField()
                );

            if (order is null)
                return Operation.Error("Order not found!");

            if (_workflowMap is null)
                return Operation.Error("WorkflowMap cannot be null!");

            var startStageCode = _workflowMap.FirstOrDefault(x => x.Position == 1)?.ApprovalStageCode;

            if (startStageCode is null)
                return Operation.Error("WorkflowMap. Start stage not found!");

            List<ApprovalWorkflow> workflows = [];

            var startStage = await _unitOfWork.GetRepository<ApprovalStage>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == startStageCode && x.Enabled,
                    trackingType: TrackingType.Tracking
                );

            var completeStage = await _unitOfWork.GetRepository<ApprovalStage>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == ApprovalWorkflow.CompletedStageCode && x.Enabled,
                    trackingType: TrackingType.Tracking
                );

            if (startStage is null)
                return Operation.Error("Start stage not found!");

            if (completeStage is null)
                return Operation.Error("Complete stage not found!");

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == "TECHNICAL_USER",
                    trackingType: TrackingType.Tracking
                );

            if (user is null)
                return Operation.Error("TECHNICAL_USER not found!");

            var orderItemModels = order.OrderItems.Where(x => x.Enabled && model.Modules.Select(m => m.Module.ModuleCode).Contains(x.Module.Code));

            //only for OrderItems that are present in the input model
            foreach (var item in orderItemModels)
            {
                var workflowCreateResult = ApprovalWorkflow.Create("DEFAULT", Guid.NewGuid().ToString(), item, item.Module.IsCustom ? startStage : completeStage, user);

                if (!workflowCreateResult.Ok)
                    return Operation.Error($"Failed to create order workflow: OrderCode = {model.Code}, OrderItemId = {item.Id}, ModuleCode = {item.Module.Code}");

                workflows.Add(workflowCreateResult.Result);
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.GetRepository<ApprovalWorkflow>().InsertAsync(workflows, cancellationToken);

            var result = await _unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync(cancellationToken);

            if (_unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return order;
        }
    }
}
