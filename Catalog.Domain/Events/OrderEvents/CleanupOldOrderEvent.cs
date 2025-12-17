using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.OrderEvents
{
    public sealed record CleanupOldOrderEvent(Int32 ArchiveStorageDays) : IOrderQueueEvent
    {
    }
}
