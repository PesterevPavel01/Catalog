using Catalog.Contracts.Dto.Module;

namespace Catalog.Contracts.Dto.Order
{
    public sealed class OrderItemDto
    {
        public required ModuleDto Module { get; set; }
        public short Quantity { get; set; }
        public bool Messages { get; set; }
    }
}
