using Catalog.Contracts.Interfaces;

namespace Catalog.Contracts.Commands
{
    public sealed record UpdateOrderCodeCommand(string Code, string NewCode) : IOrderQueueEvent;
}
