using System.ComponentModel.DataAnnotations;

namespace Catalog.Contracts.Dto
{
    public record NumericParameterDto
    {
        [Required]
        public string Type { get; set; }

        [Required]
        public string TypeCode { get; set; }

        public double Value { get; set; }
    }
}
