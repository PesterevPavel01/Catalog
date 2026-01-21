using Catalog.Contracts.Dto.Exchange;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.ApplicationEvents
{
    public sealed record RejectedEntitiesEvent(ExportedEntitiesDto Entities) : IExchangeQueueEvent;
}
