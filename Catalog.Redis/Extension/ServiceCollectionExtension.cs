using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Redis.Extension
{
    public static class ServiceCollectionExtension
    {
        public static void AddRedis(this IServiceCollection services)
        {
            InitServices(services);
        }

        private static void InitServices(this IServiceCollection services)
        {
            services.AddScoped<RedisServiceFactory>();
        }
    }
}
