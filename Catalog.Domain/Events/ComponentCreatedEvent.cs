using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events
{
    public sealed record ComponentCustomizedEvent(String componentCode) : IExchangeQueueEvent
    {
    }
}
