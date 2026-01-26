namespace Catalog.Contracts.Dto.Order
{
    public sealed record LatestChangesOrderDto
    {
        public string Code { get; set; } = default(Guid).ToString();

        public IEnumerable<OrderDto> Orders { get; set; } = [];
    }
}
