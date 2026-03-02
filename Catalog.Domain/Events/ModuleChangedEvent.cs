using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events;

public sealed record ModuleChangedEvent(Guid ModuleId) : IModuleQueueEvent;
