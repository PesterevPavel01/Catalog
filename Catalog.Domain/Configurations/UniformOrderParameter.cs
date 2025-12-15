namespace Catalog.Contracts.Configurations
{
    public sealed record UniformOrderParameter
    {
        public List<string>? TargetComponentTypes { get; set; }
        public required string Parameter { get; set; }
    }
}
