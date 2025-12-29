using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.Approval
{
    public sealed record WorkflowsCancelledEvent(OrderDto Order) : IApprovalQueueEvent
    {
    }
}
