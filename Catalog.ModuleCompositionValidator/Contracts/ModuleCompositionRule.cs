namespace Catalog.ModuleCompositionValidator.Contracts
{
    public sealed class ModuleCompositionRule
    {
        public required string ModuleParameter { get; set; }
        public required List<ModuleCompositionDependensy> Dependencies { get; set; }
    }
    public sealed class ModuleCompositionDependensy
    {
        public List<string>? TargetComponentTypes { get; set; }
        public required string Parameter { get; set; }
        public required string ComparisonRule { get; set; }
    }
}
