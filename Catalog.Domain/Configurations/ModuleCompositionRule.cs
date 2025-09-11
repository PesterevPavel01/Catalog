namespace Catalog.Contracts.Configurations
{
    public sealed class ModuleCompositionRule
    {
        public required string ModuleParameter { get; set; }
        public required List<ModuleCompositionDependency> Dependencies { get; set; }
    }
    public sealed class ModuleCompositionDependency
    {
        public List<string>? TargetComponentTypes { get; set; }
        public required string Parameter { get; set; }
        public required string ComparisonRule { get; set; }
    }
}
