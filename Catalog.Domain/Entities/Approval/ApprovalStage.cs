using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Base;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Contracts.Entities.Approval
{
    public sealed class ApprovalStage : SimpleEntity
    {
        private readonly List<ApprovalWorkflowItem> _approvalWorkflowItems = [];

        protected ApprovalStage(TitleValue title, string code, Guid id) : base(title, code, id)
        {
        }

        public IReadOnlyCollection<ApprovalWorkflowItem> ApprovalWorkflowItems => _approvalWorkflowItems.AsReadOnly();

        public static Operation<ApprovalStage, string> Create(string title, string code)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Operation.Error("Title is empty or null");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Operation.Error("Code is empty or null");
            }

            var titleResult = TitleValue.Create(title);

            if (!titleResult.Ok)
                return Operation.Error(titleResult.Error);

            return new ApprovalStage(titleResult.Result, code, Guid.NewGuid());
        }

        public SimpleEntityDto ConvertToDto()
            => new() { Code = Code, Title = Title.Value };
    }
}
