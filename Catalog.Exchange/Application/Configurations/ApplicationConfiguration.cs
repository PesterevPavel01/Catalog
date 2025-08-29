using Catalog.Contracts.Entities.Parameters;

namespace Catalog.ExchangeService.Application.Configurations
{
    public sealed record ApplicationConfiguration 
    {
        public List<ComponentRequaredRarameter> ComponentRequaredParameters { get; set; }

        public List<ComponentRequaredRarameter> CustomComponentRequaredParameters { get; set; }

        public List<String> ComponentMultipleParameters { get; set; }
    }
}
