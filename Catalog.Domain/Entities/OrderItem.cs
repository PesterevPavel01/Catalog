using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;

namespace Catalog.Domain.Entities
{
    public class OrderItem : Entity
    {
        protected OrderItem(Guid id, Int16 quantity) : base(id)
        {
            Quantity = quantity;
        }

        public Int16 Quantity { get; private set; }

        public Module Module { get; private set; } = null!;
        public Guid ModuleId { get; private set; }

        public Order Order { get; private set; } = null!;
        public Guid OrderId { get; private set; }

        public static Operation<OrderItem, string> Create(Int16 quantity, Module? module)
        {
            if (quantity < 1)
               return Operation.Error("The value cannot be less than 1");

            if (module is null)
                return Operation.Error("Module is null");

            return new OrderItem(Guid.Empty, quantity).SetModule(module);
        }

        public OrderItem SetModule(Module module)
        {
            Module = module;
            return this;
        }
    }
}
