using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.Approval;

public sealed record ApprovalCompletedEvent(String OrderCode) : IApprovalQueueEvent;
