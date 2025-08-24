using System.ComponentModel.DataAnnotations;

namespace Catalog.Contracts.Dto
{
    public record TextParameterDto
    {
        [Required]
        public string Type { get; set; }
        
        [Required]
        public string TypeCode { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
