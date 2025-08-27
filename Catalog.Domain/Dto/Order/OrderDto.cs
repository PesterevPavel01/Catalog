namespace Catalog.Contracts.Dto.Order
{
    public sealed class OrderDto
    {
        public required string Code { get; set; }
        public required string UserName { get; set; }
        public List<OrderItemDto> Modules { get; set; } = [];
    }
}
