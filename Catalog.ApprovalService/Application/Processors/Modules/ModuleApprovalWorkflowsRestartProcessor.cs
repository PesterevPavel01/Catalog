using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Application.Processors
{
    public class ModuleApprovalWorkflowRestartProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<ApprovalWorkflowMapItem> _workflowMap;

        public ModuleApprovalWorkflowRestartProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _workflowMap = applicationConfiguration.Value.ApprovalWorkflowMap;
        }

        /// <summary>
        /// restarts the ApprovalWorkflow associated with the module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>module that triggered the update</returns>
        public async Task<Operation<Module, string>> ProcessAsync(Guid moduleId, CancellationToken cancellationToken)
        {
            var module = await _unitOfWork
                .GetRepository<Module>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Id == moduleId && x.Enabled,
                    trackingType: TrackingType.Tracking,
                    include: Module.IncludeRequiredField());

            if (module is null)
                return Operation.Error("Module not found!");

            if (!module.OrderItems.Where(x => x.Enabled).Any())
                return Operation.Error("OrderItems not found!");

            //Если нет OrderItems, у которых запущен Workflow, то ничего не делаем
            if (!module.OrderItems.Where(x => x.Enabled && x.ApprovalWorkflow is not null).Select(x => x.ApprovalWorkflow).Any())
                return Operation.Error("Information: Active ApprovalWorkflows not found!");

            if (_workflowMap is null)
                return Operation.Error("WorkflowMap cannot be null!");

            var startStageCode = _workflowMap.FirstOrDefault(x => x.Position == 1)?.ApprovalStageCode;

            if (startStageCode is null)
                return Operation.Error("WorkflowMap. Start stage not found!");

            ApprovalStage? stage;

            if (module.IsCustom)
            {
                var startStage = await _unitOfWork.GetRepository<ApprovalStage>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == startStageCode && x.Enabled,
                        trackingType: TrackingType.Tracking
                    );

                if (startStage is null)
                    return Operation.Error("Start stage not found!");

                stage = startStage;
            }
            else
            {
                var completeStage = await _unitOfWork.GetRepository<ApprovalStage>()
                   .GetFirstOrDefaultAsync(
                       predicate: x => x.Code == ApprovalWorkflow.CompletedStageCode && x.Enabled,
                       trackingType: TrackingType.Tracking
                   );

                if (completeStage is null)
                    return Operation.Error("Complete stage not found!");

                stage = completeStage;
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.UserName == "TECHNICAL_USER",
                    trackingType: TrackingType.Tracking
                );

            if (user == null)
                return Operation.Error("TECHNICAL_USER not found!");

            foreach (var item in module.OrderItems.Where(x => x.Enabled))
            {
                var workflowChangeResult = item.ApprovalWorkflow.Approve(user, stage);

                if (!workflowChangeResult.Ok)
                    if (workflowChangeResult.Error != "Stage already activated!")
                        return Operation.Error(workflowChangeResult.Error);
                    else
                        continue;

                await _unitOfWork.GetRepository<ApprovalWorkflowItem>().InsertAsync(workflowChangeResult.Result, cancellationToken);
            }

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            return module;
        }
    }
}