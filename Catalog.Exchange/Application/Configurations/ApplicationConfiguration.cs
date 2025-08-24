using Catalog.Contracts.Entities.Parameters;

namespace Catalog.ExchangeService.Application.Configurations
{
    public sealed record ApplicationConfiguration 
    {
        public List<ComponentRequaredRarameter> ComponentRequaredProperties { get; set; }
    }
}
