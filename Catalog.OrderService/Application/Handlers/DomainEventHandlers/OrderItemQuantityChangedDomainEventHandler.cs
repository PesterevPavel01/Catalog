using Catalog.Contracts.DomainEvents;
using MediatR;

namespace Catalog.OrderService.Application.Handlers.DomainEventHandlers;

public class OrderItemQuantityChangedDomainEventHandler : INotificationHandler<OrderItemQuantityChangedDomainEvent>
{
    public async Task Handle(OrderItemQuantityChangedDomainEvent changeModuleQuantityDomainEvent, CancellationToken cancellationToken)
    {
    }
}
