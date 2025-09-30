using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events
{
    public sealed record ModuleCreatedEvent(Guid ModuleId) : IModuleConfigurationQueueEvent
    {

    }
}
