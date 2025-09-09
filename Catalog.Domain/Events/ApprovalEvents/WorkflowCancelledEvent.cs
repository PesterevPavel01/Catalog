using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.Approval
{
    public sealed record WorkflowCancelledEvent(String WorkflowCode) : IApprovalQueueEvent
    {
    }
}
