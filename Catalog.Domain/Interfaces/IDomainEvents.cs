using Catalog.Contracts.DomainEvents;
using MediatR;
using System.Text.Json.Serialization;

namespace Catalog.Contracts.Interfaces
{
    [JsonDerivedType(typeof(OrderCreatedDomainEvent), typeDiscriminator: "order-created")]
    [JsonDerivedType(typeof(OrderChangedDomainEvent), typeDiscriminator: "order-changed")]
    public interface IDomainEvent : INotification { }
}
