using Microsoft.Extensions.DependencyInjection;

namespace Catalog.ModuleCompositionValidator.Extension
{
    public static class ServiceCollectionExtension
    {
        public static void AddFactories(this IServiceCollection services)
        {
            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {
        }
    }
}
