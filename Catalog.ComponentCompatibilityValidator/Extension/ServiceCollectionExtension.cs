using Microsoft.Extensions.DependencyInjection;

namespace Catalog.ComponentCompatibilityValidator.Extension
{
    public static class ServiceCollectionExtension
    {
        public static void AddComponentCompabilityValidator(this IServiceCollection services)
        {
            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {
            services.AddScoped<CompabilityValidator>();
        }
    }
}
