using Calabonga.OperationResults;
using Catalog.Domain.Entities.Authorization;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using Catalog.Contracts.Dto.Order;

namespace Catalog.Domain.Entities
{
    public class Order : SimpleEntity
    {
        private readonly List<OrderItem> _orderItems = [];

        protected Order(TitleValue title, string code, Guid id) : base(title, code, id)
        {
        }

        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        public ApplicationUser ApplicationUser { get; private set; }
        public Guid ApplicationUserId { get; private set; }

        public static Operation<Order, string> Create(string? title, string? code, ApplicationUser user, List<OrderItem> orderItems)
        {
            var titleValue = TitleValue.Create(title ?? Guid.NewGuid().ToString());

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            if (orderItems is null || orderItems.Count < 1)
                return Operation.Error("OrderItems Null or Empty");

            var order = new Order(titleValue.Result, code ?? Guid.NewGuid().ToString(), Guid.Empty).SetUser(user);

            orderItems.ForEach(item => order.AddOrderItem(item));

            return order; 
        }

        private Order SetUser(ApplicationUser user) 
        {
            ApplicationUser = user;
            return this;
        }

        public void AddOrderItem(OrderItem orderItem)
        {
            var exists = _orderItems.Find(x => x.Id == orderItem.Id);
            if (exists is not null)
                return;

            _orderItems.Add(orderItem);
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
                UserName = ApplicationUser.UserName
            };
    }
}
