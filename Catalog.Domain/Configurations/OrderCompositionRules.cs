namespace Catalog.Contracts.Configurations
{
    public sealed record OrderCompositionRules
    {
        public required List<UniformOrderParameter> UniformOrderParameters { get; set; }
    }
}
