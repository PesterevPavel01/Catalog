using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Interfaces;

namespace Catalog.ComponentService.Application.Command
{
    public record SetComponentsInCacheCommand(string CacheKey, List<ComponentDto> Components) : IComponentQueueEvent;
}
