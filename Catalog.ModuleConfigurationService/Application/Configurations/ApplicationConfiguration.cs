using Catalog.ComponentCompabilityValidator.Contracts;
using Catalog.Contracts.Entities.Parameters;
using Catalog.ModuleCompositionValidator.Contracts;

namespace Catalog.ModuleConfigurationService.Application.Configurations
{
    public sealed record ApplicationConfiguration
    {
        public required List<ComponentCompabilityRule> ComponentCompabilityRules { get; set; }

        public required List<ModuleRequaredParameter> ModuleRequaredParameters { get; set; }

        public required List<ModuleCompositionRule> ModuleCompositionRules { get; set; }
    }
}
