namespace Catalog.Contracts.Dto.Components
{
    public class ComponentDto
    {
        public string ComponentCode { get; set; } = null!;
        public string ComponentTitle { get; set; } = null!;
        public string ComponentTypeCode { get;  set; } = null!;
        public string ComponentTypeTitle { get;  set; } = null!;
        public bool IsCustom { get; set; }

        public List<TextParameterDto> TextParameters { get; set; } = [];

        public List<NumericParameterDto> NumericParameters { get; set; } = [];
    }
}
