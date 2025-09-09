using Catalog.Contracts.Dto.Base;

namespace Catalog.Contracts.Dto.Approval
{
    public sealed record ApprovalWorkflowItemDto
    {
        public required int Number { get; set; }
        public required SimpleEntityDto ApprovalStage { get; set; }
    }
}
