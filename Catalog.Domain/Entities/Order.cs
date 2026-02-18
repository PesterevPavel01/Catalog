using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities.Authorization;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Catalog.Domain.Entities
{
    public class Order : SimpleEntity
    {
        public static Int16 CacheDays = 60;

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

        public bool IsApprovalCompleted() => OrderItems.Any() && OrderItems.FirstOrDefault(item => item.ApprovalWorkflow is null || item.ApprovalWorkflow.IsCompleted == false) is null;

        public bool IsCustom => CheckCustomization();

        public static Operation<Order, string> Create(string? title, string? code, ApplicationUser user)
        {
            var titleValue = TitleValue.Create(title ?? Guid.NewGuid().ToString());

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            var order = new Order(titleValue.Result, code ?? Guid.NewGuid().ToString(), Guid.Empty).SetUser(user);

            return order; 
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

        public Order AddOrderEvent(OrderEvent orderEvent) 
        {
            _orderHistory.Add(orderEvent);

            var newStatus = DetermineStatusFromEvent((OrderEventTypes)orderEvent.Type);

            if (newStatus is not null)
                SetStatus((OrderStatus)newStatus);

            return this;
        }

        public Operation<bool, string> AddOrderItem(OrderItem orderItem, IOrderValidator validator)
        {
            var exists = _orderItems.Find(x => x.Id == orderItem.Id || x.Module.Code == orderItem.Module.Code);
            //если у заказа есть уже OrderItem с этим модулем, то нужно не добавлять новый, а увеличивать Quantity у существующего!
            if (exists is not null)
                return Operation.Error("The order already has an item containing this module!");

            _orderItems.Add(orderItem);

            return validator.Validate(this);
        }

        public Order UpdateCode(string newCode) 
        {
            Code = newCode;
            return this;
        }

        public void RemoveOrderItem(OrderItem orderItem)
        {
            var exists = _orderItems.Find(x => x.Id == orderItem.Id);
            if (exists is null)
                return;

            _orderItems.Remove(orderItem);
        }

        public static Func<IQueryable<Order>, IIncludableQueryable<Order, object>> IncludeRequiredField()
            => query => query
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
                    .ThenInclude(oi => oi.Roles);

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

        public Operation<bool, string> Validate(IOrderValidator validator) 
            => validator.Validate(this);

        private bool CheckCustomization()
        => OrderItems.FirstOrDefault(x => x.Module.IsCustom) is not null;

        private OrderStatus? DetermineStatusFromEvent(OrderEventTypes eventType)
        {
            return eventType switch
            {
                OrderEventTypes.Created => OrderStatus.Draft,

                OrderEventTypes.CreateApprovalWorkflow => OrderStatus.PendingApproval,

                OrderEventTypes.Cancelled => OrderStatus.Draft,

                OrderEventTypes.ApprovalCompleted => OrderStatus.ApprovalCompleted,

                OrderEventTypes.Exported => OrderStatus.InProduction,

                OrderEventTypes.ExternallyRejected => OrderStatus.RejectedFromProduction,

                OrderEventTypes.Produced => OrderStatus.Produced,

                OrderEventTypes.Completed => OrderStatus.Completed,

                _ => null
            };
        }

        public static string GenerateUserCommonCacheKey(Func<(string Key, object Value)[], string> generateCacheKey, string userName) 
            => generateCacheKey([("type", userName), ("days", CacheDays)]);

        public static string GenerateConstructorCommonCacheKey(Func<(string Key, object Value)[], string> generateCacheKey)
            => generateCacheKey([("type", "constructor"), ("days", CacheDays)]); 

        public static string GenerateOrderCacheKey(Func<(string Key, object Value)[], string> generateCacheKey, string orderCode)
            => generateCacheKey([("type", "order"), ("code", orderCode)]); 

    }
}
