using Calabonga.OperationResults;
using Catalog.Contracts.DomainEvents;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Entities.Base;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities.Authorization;
using Catalog.Domain.ValueObjects;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Catalog.Domain.Entities
{
    public class Order : AggregateRoot
    {
        public const Int16 CacheDays = 60;

        public const String CompletionTriggerEventType = "Produced";

        private readonly List<OrderItem> _orderItems = [];

        private readonly List<OrderEvent> _orderHistory = [];

        protected Order(TitleValue title, string code, Guid id) : base(title, code, id)
        {
        }

        public OrderStatus Status { get; private set; } = OrderStatus.Draft;

        public IReadOnlyCollection<OrderEvent> OrderHistory => _orderHistory.AsReadOnly();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        public ApplicationUser ApplicationUser { get; private set; }
        public Guid ApplicationUserId { get; private set; }

        public bool IsCompleted() => Status == OrderStatus.Completed;

        public static Expression<Func<Order, bool>> IsCompletedBefore(int archiveStorageDays) 
            =>
            order => order.OrderHistory
                .Any(x => x.Type == (int)OrderEventType.Completed &&
                          x.CreatedAt < DateTime.Now.AddDays(-archiveStorageDays));

        public static Expression<Func<Order, bool>> IsInactiveBefore(int archiveStorageDays)
            =>
                order => !order.OrderHistory
                    .Any(x =>x.CreatedAt >= DateTime.Now.AddDays(-archiveStorageDays));

        public static Expression<Func<Order, bool>> IsDisableBefore(int archiveStorageDays)
            =>
               order => !order.Enabled && order.UpdatedAt < DateTime.Now.AddDays(-archiveStorageDays);

        /// <summary>
        /// Returns a combined predicate (OR) for all deletion categories
        /// </summary>
        /// <param name="completedDays"></param>
        /// <param name="inactiveDays"></param>
        /// <param name="disabledDays"></param>
        /// <returns></returns>
        public static Expression<Func<Order, bool>> GetCombinedCleanupPredicate(
            int completedDays,
            int inactiveDays,
            int disabledDays)
        {
            var predicate = PredicateBuilder.New<Order>(false);

            predicate = predicate.Or(IsCompletedBefore(completedDays));
            predicate = predicate.Or(IsInactiveBefore(inactiveDays));
            predicate = predicate.Or(IsDisableBefore(disabledDays));

            return predicate;
        }

        public bool IsApprovalCompleted() => OrderItems.Any() && OrderItems.FirstOrDefault(item => item.ApprovalWorkflow is null || item.ApprovalWorkflow.IsCompleted == false) is null;

        public bool IsCustom => CheckCustomization();

        public static Operation<Order, string> Create(string? title, string? code, ApplicationUser user, OrderEvent orderEvent)
        {
            var titleValue = TitleValue.Create(title ?? Guid.NewGuid().ToString());

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            var orderResult = new Order(titleValue.Result, code ?? Guid.NewGuid().ToString(), Guid.Empty)
                .SetUser(user)
                .AddOrderEvent(orderEvent);

            if (!orderResult.Ok) 
                return Operation.Error(orderResult.Error);

            orderResult.Result.RaiseDomainEvent(new OrderCreatedDomainEvent(orderResult.Result.Code));

            return orderResult; 
        }

        public Operation<bool, string> Disable()
        {
            base.Disable();

            var orderEvent = OrderEvent.Create(OrderEventTypeTitles.Disabled, OrderEventType.Disabled, null);

            if (!orderEvent.Ok)
                return Operation.Error(orderEvent.Error);

            AddOrderEvent(orderEvent.Result);

            RaiseDomainEvent(new OrderDisabledDomainEvent(Id));

            return true;
        }
        public Operation<Order, string> SendToProduction()
        {
            var orderEvent = OrderEvent.Create(OrderEventTypeTitles.InProduction, OrderEventType.InProduction, null);

            if (!orderEvent.Ok)
                return Operation.Error(orderEvent.Error);

            AddOrderEvent(orderEvent.Result);

            RaiseDomainEvent(new OrderInProductionDomainEvent(Id));

            return this;
        }

        public Operation<Order, string> CompleteProduction()
        {
            var orderEvent = OrderEvent.Create(OrderEventTypeTitles.Produced, OrderEventType.Produced, null);

            if(!orderEvent.Ok)
                return Operation.Error(orderEvent.Error);

            AddOrderEvent(orderEvent.Result);

            RaiseDomainEvent(new CompleteProductionDomainEvent(Code));

            return this;
        }

        private Order SetStatus(OrderStatus status)
        {
            Status = status;
            return this;
        }

        private Order SetUser(ApplicationUser user) 
        {
            ApplicationUser = user;
            return this;
        }

        public Operation<Order, string> AddOrderEvent(OrderEvent orderEvent) 
        {
            _orderHistory.Add(orderEvent);

            var newStatus = DetermineStatusFromEvent((OrderEventType)orderEvent.Type);

            if (newStatus is not null)
                SetStatus((OrderStatus)newStatus);

            //если произошло событие, которое должно автоматически завершить заказ
            if (System.Enum.TryParse<OrderEventType>(CompletionTriggerEventType, out var completionTriggerEventType))
            {
                if ((OrderEventType)orderEvent.Type == completionTriggerEventType)
                {
                    var completeResult = Complete();

                    if (!completeResult.Ok)
                        return Operation.Error(completeResult.Error);
                }
            }

            return this;
        }

        public Order UpdateCode(string newCode) 
        {
            Code = newCode;
            return this;
        }

        public Operation<bool, string> RemoveOrderItem(OrderItem orderItem)
        {
            var exists = _orderItems.Find(x => x.Id == orderItem.Id);
            
            if (exists is null)
                return Operation.Error("Order item not found!");

            if (IsApprovalCompleted())
                return Operation.Error("Order is completed!");

            var removeOrderItemEvent = OrderEvent.Create(OrderEventTypeTitles.OrderItemRemoved, OrderEventType.OrderItemRemoved, null);

            if (!removeOrderItemEvent.Ok)
                return Operation.Error(removeOrderItemEvent.Error);

            AddOrderEvent(removeOrderItemEvent.Result);

            _orderItems.Remove(orderItem);

            RaiseDomainEvent(new RemoveOrderItemDomainEvent(Id));

            //Если был удален единственный элемент с кастомным модулем 
            if (IsApprovalCompleted())
            {
                var completeApprovalResult = ApprovalComplete();

                if (!completeApprovalResult.Ok)
                    return Operation.Error(completeApprovalResult.Error);
            }    
                
            return true;
        }

        public Operation<bool, string> Complete()
        {
            var completedEvent = OrderEvent.Create(OrderEventTypeTitles.Completed, OrderEventType.Completed, null);

            if (!completedEvent.Ok)
                return Operation.Error(completedEvent.Error);

            AddOrderEvent(completedEvent.Result);

            RaiseDomainEvent(new OrderCompletedDomainEvent(Id));

            return true;
        }

        public Operation<bool, string> Cancel()
        {
            var cancelledEvent = OrderEvent.Create(OrderEventTypeTitles.Cancelled, OrderEventType.Cancelled, null);

            if (!cancelledEvent.Ok)
                return Operation.Error(cancelledEvent.Error);

            AddOrderEvent(cancelledEvent.Result);

            RaiseDomainEvent(new OrderCancelledDomainEvent(Id));

            return true;
        }

        public Operation<bool, string> Reject()
        {
            var approvalEvent = OrderEvent.Create(OrderEventTypeTitles.Reject, OrderEventType.Reject, null);

            if (!approvalEvent.Ok)
                return Operation.Error(approvalEvent.Error);

            AddOrderEvent(approvalEvent.Result);

            RaiseDomainEvent(new OrderRejectedDomainEvent(Id));
                
            return true;
        }

        public Operation<bool, string> RejectFromProduction()
        {
            var approvalEvent = OrderEvent.Create(OrderEventTypeTitles.ExternallyReject, OrderEventType.ExternallyRejected, null);

            if (!approvalEvent.Ok)
                return Operation.Error(approvalEvent.Error);

            AddOrderEvent(approvalEvent.Result);

            RaiseDomainEvent(new OrderRejectFromProductionDomainEvent(Id));

            return true;
        }

        public Operation<bool, string> ApprovalComplete()
        {
            //TODO Непонятный метод, проверить
            var approvalEvent = OrderEvent.Create(OrderEventTypeTitles.ApprovalCompleted, OrderEventType.ApprovalCompleted, null);

            if (!approvalEvent.Ok)
                return Operation.Error(approvalEvent.Error);

            AddOrderEvent(approvalEvent.Result);

            RaiseDomainEvent(new ApprovalCompletedDomainEvent(Id));

            return true;
        }

        public Operation<bool, string> Validate(IOrderValidator validator) 
            => validator.Validate(this);

        private bool CheckCustomization()
        => OrderItems.FirstOrDefault(x => x.Module.IsCustom) is not null;

        public static string GenerateUserCommonCacheKey(Func<(string Key, object Value)[], string> generateCacheKey, string userName) 
            => generateCacheKey([("type", userName), ("days", CacheDays)]);

        public static string GenerateConstructorCommonCacheKey(Func<(string Key, object Value)[], string> generateCacheKey)
            => generateCacheKey([("type", "constructor"), ("days", CacheDays)]); 

        public static string GenerateOrderCacheKey(Func<(string Key, object Value)[], string> generateCacheKey, string orderCode)
            => generateCacheKey([("type", "order"), ("code", orderCode)]);

        #region OrderItem

        public Operation<bool, string> AddOrderItem(OrderItem orderItem, IOrderValidator validator, IOrderExtendabilityValidator extendabilityValidator)
        {
            var validationResult = extendabilityValidator.Validate(this);

            if (!validationResult.Ok)
                return Operation.Error(validationResult.Error);

            var exists = _orderItems.Find(x => x.Id == orderItem.Id || x.Module.Code == orderItem.Module.Code);

            //если у заказа есть уже OrderItem с этим модулем, то нужно не добавлять новый, а увеличивать Quantity у существующего!
            if (exists is not null)
                return Operation.Error("The order already has an item containing this module!");

            var addOrderItemEvent = OrderEvent.Create(OrderEventTypeTitles.OrderItemAdded, OrderEventType.OrderItemAdded, null);

            if (!addOrderItemEvent.Ok)
                return Operation.Error(addOrderItemEvent.Error);

            AddOrderEvent(addOrderItemEvent.Result);

            RaiseDomainEvent(new AddOrderItemDomainEvent(Id));

            _orderItems.Add(orderItem);

            return validator.Validate(this);
        }

        public Operation<Order, string> AddMessageToOrderItem(OrderItem item, Message message)
        {
            if(!OrderItems.Contains(item))
                return Operation.Error($"OrderItem with id {item.Id} not found!");

            item.AddMessage(message);

            var addMessageEvent = OrderEvent.Create(OrderEventTypeTitles.MessageAdded, OrderEventType.MessageAdded, null);

            if (!addMessageEvent.Ok)
                return Operation.Error(addMessageEvent.Error);

            AddOrderEvent(addMessageEvent.Result);

            RaiseDomainEvent(new AddMessageDomainEvent(Code));

            return this;
        }

        public Operation<Order, string> ChangeItemQuantity(OrderItem item, short quantity)
        {
            if(item.Quantity == quantity)
                return this;

            item.ChangeQuantity(quantity);

            var changeItemQuantityEvent = OrderEvent.Create(OrderEventTypeTitles.OrderItemQuantityChanged, OrderEventType.OrderItemQuantityChanged, null);

            if (!changeItemQuantityEvent.Ok)
                return Operation.Error(changeItemQuantityEvent.Error);

            AddOrderEvent(changeItemQuantityEvent.Result);

            RaiseDomainEvent(new OrderItemQuantityChangedDomainEvent(Code));

            return this;
        }

        public Operation<bool, string> ModuleChange()
        {
            var moduleChangedEvent = OrderEvent.Create(OrderEventTypeTitles.Changed, OrderEventType.Changed, null);

            if (!moduleChangedEvent.Ok)
                return Operation.Error(moduleChangedEvent.Error);

            AddOrderEvent(moduleChangedEvent.Result);

            RaiseDomainEvent(new ModuleChangedDomainEvent(Id));

            return true;
        }

        public Operation<bool, string> CreateWorkflow()
        {
            var workflowCreatedEvent = OrderEvent.Create(OrderEventTypeTitles.CreateApprovalWorkflow, OrderEventType.CreateApprovalWorkflow, null);

            if (!workflowCreatedEvent.Ok)
                return Operation.Error(workflowCreatedEvent.Error);

            AddOrderEvent(workflowCreatedEvent.Result);

            RaiseDomainEvent(new WorkflowCreatedDomainEvent(Id));

            return true;
        }

        #endregion

        public OrderDto ConvertToDto()
            => new()
            {
                Code = this.Code,
                Modules = [.. OrderItems.Select(x => x.ConvertToDto())],
                Title = this.Title.Value,
                CreatedAt = this.CreatedAt,
                UpdatedAt = this.UpdatedAt,
                User = ApplicationUser.UserName,
                IsApprovalCompleted = this.IsApprovalCompleted(),
                IsCompleted = this.IsCompleted(),
                IsCustom = this.IsCustom,
                Status = this.Status.ToRussianString()
            };

        public static Func<IQueryable<Order>, IIncludableQueryable<Order, object>> IncludeRequiredField()
            => query => query
                .Include(x => x.OrderHistory)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.Components)
                            .ThenInclude(c => c.ComponentType)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.Components)
                            .ThenInclude(c => c.ComponentNumericParameters)
                                .ThenInclude(tp => tp.ParameterType)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.Components)
                            .ThenInclude(c => c.ComponentTextParameters)
                                .ThenInclude(tp => tp.ParameterType)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.ModuleType)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.ModuleTextParameters)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.ModuleTextParameters)
                            .ThenInclude(mt => mt.ParameterType)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.ModuleNumericParameters)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Module)
                        .ThenInclude(m => m.ModuleNumericParameters)
                            .ThenInclude(mt => mt.ParameterType)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.ApprovalWorkflow)
                        .ThenInclude(aw => aw.ApprovalWorkflowItems)
                            .ThenInclude(awi => awi.ApprovalStage)
                .Include(x => x.OrderItems)
                    .ThenInclude(oi => oi.Messages)
                .Include(x => x.ApplicationUser)
                    .ThenInclude(oi => oi.Roles)
                .Include(x => x.OrderHistory);

        private static OrderStatus? DetermineStatusFromEvent(OrderEventType eventType)
        {
            return eventType switch
            {
                OrderEventType.Created => OrderStatus.Draft,

                OrderEventType.CreateApprovalWorkflow => OrderStatus.PendingApproval,

                OrderEventType.Cancelled => OrderStatus.Draft,

                OrderEventType.ApprovalCompleted => OrderStatus.ApprovalCompleted,

                OrderEventType.InProduction => OrderStatus.InProduction,

                OrderEventType.ExternallyRejected => OrderStatus.RejectedFromProduction,

                OrderEventType.Produced => OrderStatus.Produced,

                OrderEventType.Completed => OrderStatus.Completed,

                _ => null
            };
        }
    }
}
