using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.ApprovalEvents
{
    public sealed record WorkflowCreatedEvent(string orderCode) : IApprovalQueueEvent
    {
    }
}
