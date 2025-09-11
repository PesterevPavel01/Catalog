namespace Catalog.Contracts.Entities.Configurations
{
    public sealed record ModuleTechnologicalRestriction
    {
        public required string ModuleTypeCode { get; set; }
        public required string Parameter {  get; set; }
        public required double Value { get; set; }
        public required string ComparisonRule { get; set; }
    }
}
