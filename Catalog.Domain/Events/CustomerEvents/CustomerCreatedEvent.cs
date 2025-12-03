using Catalog.Contracts.Dto.Authorization;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.CustomerEvents
{
    public sealed record CustomerCreatedEvent(RegistrationUserDto UserDto) : IComponentQueueEvent
    {
    }
}
