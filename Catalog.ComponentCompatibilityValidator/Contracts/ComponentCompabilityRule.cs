namespace Catalog.ComponentCompabilityValidator.Contracts
{
    public sealed class ComponentCompabilityRule
    {
        public required string ComponentParameter { get; set; }
        public required List<ComponentDependensy> Dependencies { get; set; }
    }
    public sealed class ComponentDependensy
    {
        public List<string>? TargetComponentTypes { get; set; }
        public required string Parameter { get; set; }
        public required string ComparisonRule { get; set; }
    }
}
