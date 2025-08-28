using Catalog.Contracts.Dto.Components;

namespace Catalog.Contracts.Dto.Module
{
    public sealed class ModuleDto
    {
        public required string ModuleType { get; set; }
        public required string ModuleTypeCode { get; set; }
        public required string ModuleCode { get; set; }
        public required List<ComponentDto> Components { get; set; }
        public List<NumericParameterDto>? ModuleNumericParameters { get; set; }
        public List<TextParameterDto>? ModuleTextParameters { get; set; }
    }

    public sealed class CreateModuleDto
    {
        public required string ModuleType { get; set; }
        public required string ModuleTypeCode { get; set; }
        public string? ModuleCode { get; set; }
        public string? ModuleTitle { get; set; }
        public List<NumericParameterDto>? numericParameters { get; set; } = [];
        public List<TextParameterDto>? textParameters { get; set; } = [];
    }

    public sealed class UpdateModuleDto
    {
        public string? ModuleCode { get; set; }
        public string? ModuleTitle { get; set; }
        public required List<ComponentDto> Components { get; set; } = [];
        public List<NumericParameterDto>? numericParameters { get; set; } = [];
        public List<TextParameterDto>? textParameters { get; set; } = [];
    }
}
