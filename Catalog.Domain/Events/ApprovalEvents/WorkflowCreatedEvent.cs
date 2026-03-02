using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.ApprovalEvents;

public sealed record WorkflowCreatedEvent(OrderDto Order) : IApprovalQueueEvent;
