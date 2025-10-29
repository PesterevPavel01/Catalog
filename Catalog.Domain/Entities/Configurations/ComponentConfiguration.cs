using Catalog.Contracts.Entities.Parameters;

namespace Catalog.Contracts.Entities.Configurations
{
    public sealed class ComponentConfiguration
    {
        public IEnumerable<ComponentRequiredParameter> ComponentRequiredParameters { get; set; }

        public IEnumerable<ComponentRequiredParameter> CustomComponentRequiredParameters { get; set; }

        public IEnumerable<String> ComponentMultipleParameters { get; set; }
    }
}
