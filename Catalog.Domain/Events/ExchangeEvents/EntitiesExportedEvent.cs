using Catalog.Contracts.Dto.Exchange;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Events.ExchangeEvents
{
    public sealed record EntitiesExportedEvent(ExportedEntitiesDto Entities) : IExchangeQueueEvent;
}
