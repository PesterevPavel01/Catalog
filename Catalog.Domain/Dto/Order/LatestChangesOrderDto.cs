namespace Catalog.Contracts.Dto.Order
{
    public sealed record LatestChangesOrderDto
    {
        public string Code { get; set; } = default(Guid).ToString();

        public IEnumerable<OrderDto> CreatedOrders { get; set; } = [];

        //public IEnumerable<OrderDto> UpdatedOrders { get; set; } = [];

        //public IEnumerable<OrderDto> DeletedOrders { get; set; } = [];
    }
}
