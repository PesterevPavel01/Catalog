using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Events;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using Catalog.Contracts.Enum;

namespace Catalog.Contracts.Entities
{
    public class OrderEvent: SimpleEntity
    {
        protected OrderEvent(Int32 type, TitleValue title, string code, Guid id) : base(title, code, id)
        {
            Type = type;
        }

        public Order Order { get; private set; } = null!;
        public Guid OrderId { get; private set; }
        public Int32 Type { get; private set; }

        public static Operation<OrderEvent, string> Create(
            //Order order, 
            string title, OrderEventTypes type, string? code)
        {
            var titleValue = TitleValue.Create(title);

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            /*if (order is null)
                return Operation.Error("Order is null");*/

            var orderEvent = new OrderEvent((Int32)type, titleValue.Result, code ?? Guid.NewGuid().ToString(), Guid.Empty);
            /*{
                Order = order
            };*/

            return orderEvent;
        }

        public OrderEventDto ConvertToDto()
            => new(Order.ApplicationUser.UserName, Order.Title.Value, Order.Code, Title.Value, CreatedAt);

        public static Func<IQueryable<OrderEvent>, IIncludableQueryable<OrderEvent, object>> IncludeRequiredField()
            => query => query
                .Include(x => x.Order)
                    .ThenInclude(o => o.ApplicationUser);
    }
}
