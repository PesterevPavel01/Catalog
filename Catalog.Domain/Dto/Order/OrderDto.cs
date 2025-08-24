using Catalog.Domain.Dto;

namespace Catalog.Contracts.Dto.Order
{
    public sealed class OrderDto
    {
        public List<OrderItemDto> OrderItems { get; set; } = [];
        public required string OrderCode { get; set; }
        public string OrderTitle { get; set; } = null!;
        public required string UserName { get; set; }
    }
}
