using Catalog.Contracts.Entities.Parameters;

namespace Catalog.Contracts.Entities.Configurations
{
    public sealed class ComponentConfiguration
    {
        public IEnumerable<ComponentRequiredRarameter> ComponentRequiredParameters { get; set; }

        public IEnumerable<ComponentRequiredRarameter> CustomComponentRequiredParameters { get; set; }

        public IEnumerable<String> ComponentMultipleParameters { get; set; }
    }
}
