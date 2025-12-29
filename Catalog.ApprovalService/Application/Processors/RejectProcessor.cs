using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities.Approval;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Application.Processors
{
    public sealed class RejectProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public RejectProcessor(IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<OrderDto, string>> ProcessAsync(OrderDto model, CancellationToken cancellationToken)
        {
            var orderRepository = _unitOfWork.GetRepository<Order>();

            var order = await orderRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.Code && x.Enabled,
                    include: Order.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

            if (order is null)
                return Operation.Error("Order not found!");

            var workflows = order.OrderItems
                .Where(item => 
                    item.ApprovalWorkflow is not null
                    && item.Module.IsCustom)
                .Select(x => x.ApprovalWorkflow);

            if (!workflows.Any())
                return Operation.Error("Workflows not found!");

            var user = await _unitOfWork
                .GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == model.UserName,
                    trackingType: TrackingType.Tracking,
                    include: query => query.Include(x => x.Roles)
                );

            if (user is null)
                return Operation.Error("User not found!");

            foreach (var workflow in workflows)
            {
                if (workflow.ActiveStage is null)
                    throw new ArgumentOutOfRangeException("ApprovalWorkflowItems is null!");

                if (workflow.ActiveStage.Number == 1)
                    return Operation.Error("First stage is active!");

                var allowedApproverRoleCodes = _applicationConfiguration.Value.ApprovalWorkflowMap
                    .FirstOrDefault(x =>
                        x.ApprovalStageCode == workflow.ActiveStage.ApprovalStage.Code
                        && x.Position == workflow.ActiveStage.Number)?.AllowedApproverRoleCodes;

                if (allowedApproverRoleCodes is null)
                    return Operation.Error("Approval workflow map not found!");

                if (user.Roles.FirstOrDefault(x => allowedApproverRoleCodes.Contains(x.Code)) is null)
                    return Operation.Error("Forbidden!");

                var prevStagePosition = (short)(workflow.ActiveStage.Number - 1);

                var prevStageCode = _applicationConfiguration.Value.ApprovalWorkflowMap
                    .FirstOrDefault(x => x.Position == prevStagePosition)?.ApprovalStageCode;

                if (prevStageCode is null)
                    return Operation.Error($"Stage not found! Position: {workflow.ActiveStage.Number - 1}");

                var prevStage = await _unitOfWork.GetRepository<ApprovalStage>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == prevStageCode,
                        trackingType: TrackingType.Tracking
                    );

                if (prevStage is null)
                    return Operation.Error($"Approval Stage: Code = \"{prevStageCode}\" not found");

                var approveResult = workflow.Approve(user, prevStage, prevStagePosition);

                if (!approveResult.Ok)
                    return Operation.Error(approveResult.Error);

                var workflowResult = await _unitOfWork
                    .GetRepository<ApprovalWorkflowItem>()
                    .InsertAsync(approveResult.Result, cancellationToken);

            }

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return order.ConvertToDto();
        }
    }
}