using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.Contracts.Dto.Approval;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Entities.Configurations;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Application.Processors
{
    public sealed class OrderByRoleLoadeerProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IEnumerable<ApprovalWorkflowMapItem> _workflowMap;

        public OrderByRoleLoadeerProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _workflowMap = applicationConfiguration.Value.ApprovalWorkflowMap;
        }

        public async Task<Operation<List<ApprovalWorkflowDto>, string>> ProcessAsync(string code, CancellationToken cancellationToken)
        {
            var operationResult = await _unitOfWork.GetRepository<ApprovalWorkflow>()
                .GetAllAsync(
                    predicate: x => x.OrderItem.Order.Code == code,
                    include: ApprovalWorkflow.IncludeRequiredField()
                );

            if (!operationResult.Any())
                return new();

            return operationResult.Select(x => x.ConvertToDto()).ToList();
        }
    }
}
