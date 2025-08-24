using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events
{
    public sealed record ComponentCreatedEvent(List<String> ComponentCodes) : IExchangeQueueEvent
    {
    }
}
