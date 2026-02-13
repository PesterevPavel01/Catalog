using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Commands.Exchange
{
    public sealed record ComponentSyncCommand(IEnumerable<ComponentDto> Components, string SessionCode) : IExchangeQueueEvent;
}
