using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Dto
{
    public sealed class OrderDto
    {
        public List<OrderItemDto> OrderItems { get; set; } = [];
        public String OrderCode { get; set; } = null!;
        public String OrderTitle { get; set; } = null!;
    }
}
