using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.OrderEvents;

public sealed record CleanupOldOrderEvent() : IOrderQueueEvent;
