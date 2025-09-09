using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Base;
using Catalog.Contracts.Entities.Approval;

namespace Catalog.ApprovalService.Application.Processors
{
    public sealed class CreateApprovalStageProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateApprovalStageProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<IEnumerable<SimpleEntityDto>, string>> ProcessAsync(IEnumerable<SimpleEntityDto> models, CancellationToken cancellationToken) 
        {
            if (models is null || !models.Any())
                return Operation.Error("Model list cannot be empty!");

            var approvalStageRepository = _unitOfWork.GetRepository<ApprovalStage>();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var existingStages = await approvalStageRepository
                .GetAllAsync(
                    predicate: x=> models.Select(m => m.Code).Contains(x.Code),
                    trackingType: TrackingType.NoTracking
                );

            if (existingStages.Any())
                return Operation.Error("Some models already exist!");

            List<ApprovalStage> approvalStages = [];

            foreach (var model in models) 
            {
                var operationResult = ApprovalStage
                    .Create(
                        title: model.Title,
                        code: model.Code
                    );

                if (!operationResult.Ok)
                    return Operation.Error(operationResult.Error);

                approvalStages.Add(operationResult.Result);
            }

            await approvalStageRepository.InsertAsync(approvalStages, cancellationToken);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            await transaction.CommitAsync(cancellationToken);

            return approvalStages.Select(x=> x.ConvertToDto()).ToArray();
        }
    }
}
