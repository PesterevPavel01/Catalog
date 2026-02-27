using Catalog.Contracts.DomainEvents;
using Catalog.Contracts.Events.OrderEvents;
using MediatR;
using Rebus.Bus;

namespace Catalog.OrderService.Application.Handlers.DomainEventHandlers
{
    public class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
    {
        private readonly IBus _bus;

        public OrderCreatedDomainEventHandler(IBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(OrderCreatedDomainEvent orderCreatedDomainEvent, CancellationToken cancellationToken)
        {
            await _bus.Send(new OrderCreatedEvent(orderCreatedDomainEvent.Code));
        }
    }
}
