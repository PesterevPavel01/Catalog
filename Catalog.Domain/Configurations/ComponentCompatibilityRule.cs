namespace Catalog.Contracts.Configurations
{
    public sealed class ComponentCompatibilityRule
    {
        public required string ComponentParameter { get; set; }
        public required List<ComponentDependency> Dependencies { get; set; }
    }
    public sealed class ComponentDependency
    {
        public List<string>? TargetComponentTypes { get; set; }
        public required string Parameter { get; set; }
        public required string ComparisonRule { get; set; }
    }
}
