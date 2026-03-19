using Catalog.Contracts.Enum;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.OrderEvents;

public sealed record CommonOrderEvent (string Title, string Message, OrderEventType EventType) : IOrderQueueEvent;
