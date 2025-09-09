using Catalog.Contracts.Dto.Order;

namespace Catalog.Contracts.Dto.Approval
{
    public sealed record ApprovalWorkflowDto
    {
        public required OrderItemDto OrderItem { get; set; }
        public required ApprovalWorkflowItemDto ActiveStage { get; set; }
        public bool IsCompleted { get; set; }
    }
}
