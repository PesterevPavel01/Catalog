using Catalog.Contracts.DomainEvents;
using MediatR;
using System.Text.Json.Serialization;

namespace Catalog.Contracts.Interfaces
{
    [JsonDerivedType(typeof(OrderCreatedDomainEvent), typeDiscriminator: "order-created")]
    [JsonDerivedType(typeof(AddOrderItemDomainEvent), typeDiscriminator: "order-add-item")]
    [JsonDerivedType(typeof(AddMessageDomainEvent), typeDiscriminator: "order-add-message")]
    [JsonDerivedType(typeof(OrderItemQuantityChangedDomainEvent), typeDiscriminator: "order-item-quantity-change")]
    [JsonDerivedType(typeof(OrderDisabledDomainEvent), typeDiscriminator: "order-disabled")]
    [JsonDerivedType(typeof(CompleteProductionDomainEvent), typeDiscriminator: "order-produced")]
    [JsonDerivedType(typeof(OrderInProductionDomainEvent), typeDiscriminator: "order-in-production")]
    [JsonDerivedType(typeof(RemoveOrderItemDomainEvent), typeDiscriminator: "remove-order-item")]
    [JsonDerivedType(typeof(ApprovalCompletedDomainEvent), typeDiscriminator: "approval-completed")]
    [JsonDerivedType(typeof(WorkflowCreatedDomainEvent), typeDiscriminator: "workflow-created")]
    [JsonDerivedType(typeof(ModuleChangedDomainEvent), typeDiscriminator: "custom-module-changed")]
    [JsonDerivedType(typeof(OrderCancelledDomainEvent), typeDiscriminator: "order-cancelled")]
    [JsonDerivedType(typeof(OrderRejectedDomainEvent), typeDiscriminator: "order-rejected")]
    [JsonDerivedType(typeof(OrderRejectFromProductionDomainEvent), typeDiscriminator: "order-rejected-from-production")]
    [JsonDerivedType(typeof(OrderCompletedDomainEvent), typeDiscriminator: "order-completed")]
    


    public interface IDomainEvent : INotification { }
}
