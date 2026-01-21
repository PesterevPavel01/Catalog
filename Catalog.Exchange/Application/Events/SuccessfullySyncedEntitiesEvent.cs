using Catalog.Contracts.Dto.Exchange;
using Catalog.Contracts.Interfaces;

namespace Catalog.ExchangeService.Application.Events
{
    public sealed record SuccessfullySyncedEntitiesEvent(IEnumerable<ExternalEntityMappingDto> Models) : IExchangeQueueEvent;
}
