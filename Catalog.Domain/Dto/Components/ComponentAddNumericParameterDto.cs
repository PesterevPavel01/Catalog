namespace Catalog.Contracts.Dto.Components
{
    public sealed class ComponentAddNumericParameterDto
    {
        public required string ComponentCode { get; set; }

        public required List<NumericParameterDto> NumericParameters { get; set; }
    }
}
