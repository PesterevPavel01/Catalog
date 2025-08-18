using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public class Order : SimpleEntity
    {
        private readonly List<OrderItem> _orderItems = [];

        protected Order(TitleValue title, CodeValue code, Guid id) : base(title, code, id)
        {
        }
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public static Operation<Order, string> Create(string? title, string? code, List<OrderItem> orderItems)
        {
            var titleValue = TitleValue.Create(title ?? Guid.NewGuid().ToString());

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            var codeValue = CodeValue.Create(code ?? Guid.NewGuid().ToString());

            if (!codeValue.Ok)
                return Operation.Error(codeValue.Error);

            if (orderItems is null || orderItems.Count < 1)
                return Operation.Error("OrderItems Null or Empty");

            var order = new Order(titleValue.Result, codeValue.Result, Guid.Empty);

            orderItems.ForEach(item => order.AddOrderItem(item));

            return order; 
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
    }
}
