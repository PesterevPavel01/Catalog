using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Parameters;

namespace Catalog.Contracts.Configurations
{
    public sealed record ModuleConfiguration
    {
        public required List<ModuleTechnologicalRestriction> ModuleTechnologicalRestrictions { get; set; }
        public required List<ComponentCompatibilityRule> ComponentCompatibilityRules { get; set; }
        public required List<ModuleRequiredParameter> ModuleRequiredParameters { get; set; }
        public required List<ModuleCompositionRule> ModuleCompositionRules { get; set; }
    }
}
