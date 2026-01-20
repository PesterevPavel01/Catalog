namespace Catalog.Contracts.Dto.Order
{
    public sealed class OrderDto
    {
        public required string Code { get; set; }
        public required string User { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDto> Modules { get; set; } = [];
        public required bool IsCompleted { get; set; }
        public bool IsCustom { get; set; }

        public OrderDto SetUser(string user)
        { 
            User = user;
            return this;
        }
    }
}
