using Catalog.Domain.Entities;

namespace Catalog.Contracts.Dto
{
    public class ComponentDto
    {
        public String ComponentCode { get; set; } = null!;
        public String ComponentTitle { get; set; } = null!;
        public String ComponentTypeCode { get;  set; } = null!;
        public String ComponentTypeTitle { get;  set; } = null!;

        public List<TextParameterDto> TextParameters { get; set; } = [];

        public List<NumericParameterDto> NumericParameters { get; set; } = [];
    }
}
