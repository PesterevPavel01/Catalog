using Catalog.Contracts.Enum;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Commands
{
    public record CreateOrderEventCommand(string OrderCode, OrderEventTypes Type, string Note ) : IOrderQueueEvent;
}
