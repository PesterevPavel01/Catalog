using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Message;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Authorization;
using Catalog.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Contracts.Entities
{
    public class Message : Entity
    {
        private Message(Guid id, string text) : base(id)
        {
            Text=text;
        }

        public ApplicationUser ApplicationUser { get; private set; }
        public Guid ApplicationUserId { get; private set; }
        public OrderItem OrderItem { get; private set; }
        public Guid OrderItemId { get; private set; }
        public string Text { get; private set; }

        public static Operation<Message, string> Create(string text, OrderItem orderItem, ApplicationUser applicationUser)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Operation.Error("Text is empty or null");

            var message = new Message(Guid.Empty, text);

            message.OrderItem = orderItem;

            message.ApplicationUser = applicationUser;

            return message;
        }

        public static Func<IQueryable<Message>, IIncludableQueryable<Message, object>> IncludeRequiredField()
        => query => query
            .Include(x => x.ApplicationUser)
                .ThenInclude(x => x.Roles)
            .Include(x => x.OrderItem)
                .ThenInclude(x => x.Module)
            .Include(x => x.OrderItem)
                .ThenInclude(x => x.Order);

        public MessageDto ConvertToDto()
        => new ()
            {
                OrderCode = this.OrderItem.Order.Code,
                ModuleCode = this.OrderItem.Module.Code,
                CreatedAt = this.CreatedAt,
                Text = this.Text,
                SenderRoles = ApplicationUser.Roles.Select(x => x.Code)
            };
    }
}
