using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.ApprovalEvents
{
    public sealed record CustomWorkflowChangedEvent(Guid WorkflowId) : IApprovalQueueEvent
    {
    }
}
