using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Entities.Approval;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure
{
    public static class RequiredStageDefinition
    {
        public static async Task CreateAddRequiredStage(IServiceProvider service)
        {
            using var scope = service.CreateScope();

            var services = scope.ServiceProvider;

            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var applicationConfig = services.GetRequiredService<IOptions<ApplicationConfiguration>>().Value;

            var result = await CreateRequiredStageAsync(unitOfWork, applicationConfig);

            if (!result.Ok)
                throw new Exception(result.Error);
        }

        private static async Task<Operation<bool, string>> CreateRequiredStageAsync(IUnitOfWork unitOfWork, ApplicationConfiguration configuration)
        {
            var _workflowMap = configuration.ApprovalWorkflowMap;

            var startStageCode = _workflowMap.FirstOrDefault(x => x.Position == 1)?.ApprovalStageCode;

            if (startStageCode is null)
                throw new ArgumentOutOfRangeException("ApprovalWorkflowMap. Position #1 element not found!");

            var orderRepository = unitOfWork.GetRepository<OrderItem>();

            var startStage = await unitOfWork.GetRepository<ApprovalStage>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == startStageCode,
                    trackingType: TrackingType.Tracking
                );

            var completeStage = await unitOfWork.GetRepository<ApprovalStage>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == ApprovalWorkflow.CompletedStageCode,
                    trackingType: TrackingType.Tracking
                );

            if (startStage is null)
            {
                var stageCreationResult = ApprovalStage.Create(title: startStageCode, code: startStageCode);

                if (!stageCreationResult)
                    return Operation.Error(stageCreationResult.Error);

                startStage = stageCreationResult.Result;

                await unitOfWork.GetRepository<ApprovalStage>().InsertAsync(startStage);
            }

            if (completeStage is null)
            {
                var stageCreationResult = ApprovalStage.Create(title: ApprovalWorkflow.CompletedStageCode, code: ApprovalWorkflow.CompletedStageCode);

                if (!stageCreationResult)
                    return Operation.Error(stageCreationResult.Error);

                completeStage = stageCreationResult.Result;

                await unitOfWork.GetRepository<ApprovalStage>().InsertAsync(completeStage);
            }
            else 
                return true;

            using var transaction = await unitOfWork.BeginTransactionAsync();

            var result = await unitOfWork.SaveChangesAsync();

            await transaction.CommitAsync();

            if (unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync();
                return Operation.Error(unitOfWork.Result.Exception.Message);
            }

            return true;
        }

    }
}
