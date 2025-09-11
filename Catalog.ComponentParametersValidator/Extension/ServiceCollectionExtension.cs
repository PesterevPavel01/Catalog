using Catalog.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.ComponentParametersValidator.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddComponentParametersValidator(this IServiceCollection services)
        {
            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {
            services.AddScoped<IComponentParametersValidator, ComponentParametersValidator>();
        }
    }
}
