using Catalog.Contracts.Dto.Order;
using MediatR;

namespace Catalog.Contracts.Events.ApprovalEvents;

public sealed record WorkflowCompleteCommand(OrderDto Order) : INotification;

