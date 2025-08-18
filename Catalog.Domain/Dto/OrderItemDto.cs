using System.ComponentModel.DataAnnotations;

namespace Catalog.Domain.Dto
{
    public sealed class OrderItemDto
    {
        [Required]
        public string ModuleCode { get; set; } = null!;

        public Int16 Quantity { get; set; }
    }
}
