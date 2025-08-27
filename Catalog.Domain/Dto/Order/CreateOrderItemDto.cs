using System.ComponentModel.DataAnnotations;

namespace Catalog.Contracts.Dto.Order
{
    public sealed class CreateOrderItemDto
    {
        [Required]
        public string ModuleCode { get; set; } = null!;

        public short Quantity { get; set; }
    }
}
