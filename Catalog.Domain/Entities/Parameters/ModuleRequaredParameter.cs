namespace Catalog.Contracts.Entities.Parameters
{
    public sealed class ModuleRequaredParameter
    {
        public required string ModuleType { get; set; }
        public required List<ModuleParameterRule> Parameters { get; set; }
    }

    public sealed class ModuleParameterRule 
    {
        public required string Parameter {  get; set; }
        public List<ModuleRequaredParameterDependency>? Dependencies { get; set; }
    }

    public sealed class ModuleRequaredParameterDependency
    { 
        public string? ComponentsTitle { get; set; }
        public string? ComponentsTypeTitle { get; set; }
    }
}
