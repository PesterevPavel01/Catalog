using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events
{
    public sealed record ModuleUpdatedEvent(Guid ModuleId) : IExchangeQueueEvent
    {

    }
}
