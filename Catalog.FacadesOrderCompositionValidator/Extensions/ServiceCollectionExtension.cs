using Catalog.Contracts.Interfaces;
using Catalog.FacadesOrderCompositionValidator;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.FacadesOrderTitleValidator.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddFacadesOrderCompositionValidator(this IServiceCollection services)
        {
            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {
            services.AddScoped<IOrderValidator, CompositionValidator>();
        }
    }
}
