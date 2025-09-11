using Catalog.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.ModuleParametersValidator.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddModuleCreationValidator(this IServiceCollection services)
        {
            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {
            services.AddScoped<IModuleParametersValidator, ModuleParametersValidator>();
        }
    }
}
