using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events
{
    public sealed record OrderCreatedEvent(String OrderCode) : IOrderQueueEvent
    {

    }
}
