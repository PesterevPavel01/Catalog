using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Approval;
using Catalog.Contracts.Entities.Approval;

namespace Catalog.ApprovalService.Application.Processors
{
    public sealed class GetWorkflowsProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWorkflowsProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<List<ApprovalWorkflowDto>, string>> ProcessAsync(string code, CancellationToken cancellationToken)
        {
            var operationResult = await _unitOfWork.GetRepository<ApprovalWorkflow>()
                .GetAllAsync(
                    predicate: x => x.OrderItem.Order.Code == code && x.Enabled,
                    include: ApprovalWorkflow.IncludeRequiredField()
                );

            if (!operationResult.Any())
                return new List<ApprovalWorkflowDto>();

            return operationResult.Select(x => x.ConvertToDto()).ToList();
        }
    }
}
