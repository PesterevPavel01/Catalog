using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.ExchangeEvents
{
    public sealed record SyncFailedEvent (string Message) : IExchangeQueueEvent;
}
