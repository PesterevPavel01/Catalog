using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Dto.Approval;
using Catalog.Contracts.Entities.Approval;
using Catalog.Domain.Entities.Autorization;
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

        public async Task<Operation<ApprovalWorkflowDto, string>> ProcessAsync(string workflowCode, string userName, CancellationToken cancellationToken)
        {
            var workflowRepository = _unitOfWork.GetRepository<ApprovalWorkflow>();

            var workflow = await workflowRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == workflowCode,
                    include: ApprovalWorkflow.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

            if (workflow is null)
                return Operation.Error("Workflow not found!");

            if (workflow.ActiveStage is null)
                throw new ArgumentOutOfRangeException("ApprovalWorkflowItems is null!");

            if (workflow.ActiveStage.Number == 1)
                return Operation.Error("First stage is active!");

            var user = await _unitOfWork
                .GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == userName,
                    trackingType: TrackingType.Tracking,
                    include: query => query.Include(x => x.Roles)
                );

            if (user is null)
                return Operation.Error("User not found!");

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

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return workflow.ConvertToDto();
        }
    }
}