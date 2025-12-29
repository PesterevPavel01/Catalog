using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Commands
{
    public record CreateOrderEventCommand(string OrderCode, string Note ) : IOrderQueueEvent;
}
