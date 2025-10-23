using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Entities;
using Catalog.Contracts.Entities.Approval;
using Catalog.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Catalog.Domain.Entities
{
    public class OrderItem : Entity
    {
        private readonly List<Message> _messages = [];

        protected OrderItem(Guid id, Int16 quantity) : base(id)
        {
            Quantity = quantity;
        }

        public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

        public Int16 Quantity { get; private set; }

        public Module Module { get; private set; } = null!;
        public Guid ModuleId { get; private set; }

        public Order Order { get; private set; } = null!;
        public Guid OrderId { get; private set; }

        public ApprovalWorkflow ApprovalWorkflow { get; private set; }

        public static Operation<OrderItem, string> Create(Int16 quantity, Module? module)
        {
            if (quantity < 1)
               return Operation.Error("The value cannot be less than 1");

            if (module is null)
                return Operation.Error("Module is null");

            return new OrderItem(Guid.Empty, quantity).SetModule(module);
        }

        public void SetQuantity(Int16 quantity) 
        {
            Quantity = quantity;
        }

        public void AddMessage(Message message)
        {
            var exists = _messages.Find(x => x.Id == message.Id);
            if (exists is not null)
                return;

            _messages.Add(message);
        }

        private OrderItem SetModule(Module module)
        {
            Module = module;
            return this;
        }

        public static Func<IQueryable<OrderItem>, IIncludableQueryable<OrderItem, object>> IncludeRequiredField()
            => query => query
                .Include(oi => oi.Module)
                    .ThenInclude(m => m.Components)
                        .ThenInclude(c => c.ComponentNumericParameters)
                            .ThenInclude(tp => tp.ParameterType)
                .Include(oi => oi.Module)
                    .ThenInclude(m => m.Components)
                        .ThenInclude(c => c.ComponentTextParameters)
                            .ThenInclude(tp => tp.ParameterType)
                .Include(oi => oi.Module)
                    .ThenInclude(m => m.ModuleType);

        public OrderItemDto ConvertToDto()
        => new() 
        { 
            Module = this.Module.ConvertToDto(),
            Quantity = this.Quantity,
        };
    }
}
