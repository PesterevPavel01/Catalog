using Microsoft.Extensions.DependencyInjection;

namespace Catalog.FacadeOrderTitleValidator.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddFacadeOrderTitleValidator(this IServiceCollection services)
        {
            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {
            services.AddScoped<ITitleValidator, TitleValidator>();
        }
    }
}
