using Catalog.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.OrderExtendabilityValidator.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddOrderExtendabilityValidator(this IServiceCollection services)
        {
            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {
            services.AddScoped<IOrderExtendabilityValidator, DefaultValidator>();
        }
    }
}
