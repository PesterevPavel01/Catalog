using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Dto.Approval;
using Catalog.Contracts.Entities.Approval;
using Catalog.Domain.Entities.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Application.Processors
{
    public sealed class PermissionCheckerProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public PermissionCheckerProcessor(IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<bool, string>> ProcessAsync(string workflowCode, string userName, CancellationToken cancellationToken)
        {
            var workflowRepository = _unitOfWork.GetRepository<ApprovalWorkflow>();

            var workflow = await workflowRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == workflowCode,
                    include: ApprovalWorkflow.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

            if (workflow is null)
                return Operation.Error("Workflow not found!");

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

            return true;
        }
    }
}