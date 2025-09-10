
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
    public sealed class ApproveProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public ApproveProcessor(IOptions<ApplicationConfiguration> applicationConfiguration, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<ApprovalWorkflowDto,string>> ProcessAsync(string workflowCode, string userName, CancellationToken cancellationToken)
        {
            var workflowRepository = _unitOfWork.GetRepository<ApprovalWorkflow>();

            var workflow = await workflowRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == workflowCode,
                    include: ApprovalWorkflow.IncludeRequiredField(),
                    trackingType: TrackingType.Tracking);

            if (workflow is null) 
                return Operation.Error("Workflow not found!");

            if(workflow.ActiveStage.ApprovalStage.Code == ApprovalWorkflow.CompletedStageCode)
                return Operation.Error("Workflow already completed!");

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

            var nextStagePosition = (short)(workflow.ActiveStage.Number + 1);

            var nextStageCode = _applicationConfiguration.Value.ApprovalWorkflowMap
                .FirstOrDefault(x => x.Position == nextStagePosition)?.ApprovalStageCode;

            if (nextStageCode is null || nextStageCode == ApprovalWorkflow.CompletedStageCode)
            {
                var completedStage = await _unitOfWork.GetRepository<ApprovalStage>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == ApprovalWorkflow.CompletedStageCode,
                        trackingType: TrackingType.Tracking
                    );

                if (completedStage is null)
                {
                    var stageCreationResult = ApprovalStage.Create(title: ApprovalWorkflow.CompletedStageCode, code: ApprovalWorkflow.CompletedStageCode);

                    if (!stageCreationResult)
                        return Operation.Error(stageCreationResult.Error);

                    completedStage = stageCreationResult.Result;
                }

                var completeResult = workflow.Complete(user, completedStage);

                if (!completeResult.Ok)
                    return Operation.Error(completeResult.Error);
                
                var workflowResult = await _unitOfWork
                    .GetRepository<ApprovalWorkflowItem>()
                    .InsertAsync(completeResult.Result, cancellationToken);
            }
            else
            {
                var nextStage = await _unitOfWork.GetRepository<ApprovalStage>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == nextStageCode,
                        trackingType: TrackingType.Tracking
                    );

                if (nextStage is null)
                    return Operation.Error($"Approval Stage: Code = '{nextStageCode}' not found");

                var approveResult = workflow.Approve(user, nextStage, nextStagePosition);

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

            return workflow.ConvertToDto();
        }
    }
}
