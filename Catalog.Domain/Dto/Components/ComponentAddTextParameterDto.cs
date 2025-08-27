namespace Catalog.Contracts.Dto.Components
{
    public sealed class ComponentAddTextParameterDto
    {
        public required string ComponentCode { get; set; }

        public required List<TextParameterDto> TextParameters { get; set; }
    }
}
