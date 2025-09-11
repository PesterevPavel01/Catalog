namespace Catalog.Contracts.Entities.Parameters
{
    public sealed class ModuleRequiredParameter
    {
        public required string ModuleTypeCode { get; set; }
        public required List<ModuleParameterRule> Parameters { get; set; }
    }

    public sealed class ModuleParameterRule 
    {
        public required string Parameter {  get; set; }
        public List<ModuleRequiredParameterDependency>? Dependencies { get; set; }
    }

    public sealed class ModuleRequiredParameterDependency
    { 
        public string? ComponentsTitle { get; set; }
        public string? ComponentsTypeTitle { get; set; }
    }
}
