using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;

namespace Catalog.ApprovalService.Application.Commands
{
    public sealed record CancelWorkflowCommand(OrderDto Order) : IApprovalQueueEvent;
}
