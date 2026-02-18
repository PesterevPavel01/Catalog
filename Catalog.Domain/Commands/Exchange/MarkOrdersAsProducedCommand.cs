using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Commands.Exchange
{
    public sealed record MarkOrdersAsProducedCommand(IEnumerable<String> codes) : IExchangeQueueEvent;
}
