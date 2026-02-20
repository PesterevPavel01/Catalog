using Catalog.Contracts.Enum;
using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Commands
{
    public record CreateOrderEventCommand(string OrderCode, OrderEventType Type, string Note ) : IOrderQueueEvent;
}
