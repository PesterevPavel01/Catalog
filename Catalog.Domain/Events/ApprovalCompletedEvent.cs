using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events
{
    public sealed record ApprovalCompletedEvent(Guid OrderId, Guid ModuleId) : IExchangeQueueEvent
    {
    }
}
