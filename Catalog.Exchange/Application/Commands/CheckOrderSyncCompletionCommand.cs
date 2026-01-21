using Catalog.Contracts.Interfaces;

namespace Catalog.ExchangeService.Application.Commands
{
    public record CheckOrderSyncCompletionCommand(Guid TransactionId) : IExchangeQueueEvent
    {
    }
}
