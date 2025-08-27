namespace Catalog.Contracts.Dto.Order
{
    public sealed class CreateOrderDto
    {
        public List<CreateOrderItemDto> OrderItems { get; set; } = [];
        public string? OrderCode { get; set; }
        public string? OrderTitle { get; set; }
        public required string UserName { get; set; }
    }
}
