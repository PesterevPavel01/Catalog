using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Rabbit;

namespace Catalog.ExchangeService.Definitions.Configurations
{
    public static class ConfigurationDefinition
    {
        public static void AddSharedConfiguration(this WebApplicationBuilder builder)
        {
            ConfigureAuthorization(builder);

            ConfigureApplication(builder);

            ConfigureRabbit(builder);
        }

        private static void ConfigureApplication(WebApplicationBuilder builder)
        {
            builder.Services.Configure<ComponentConfiguration>(
                builder.Configuration.GetSection("ApplicationConfiguration"));
        }

        private static void ConfigureAuthorization(WebApplicationBuilder builder)
        {
            builder.Services.Configure<AuthorizationSettings>(
                builder.Configuration.GetSection("Authorization"));
        }
        private static void ConfigureRabbit(WebApplicationBuilder builder)
        {
            builder.Services.Configure<RabbitSettings>(
                builder.Configuration.GetSection("RabbitMq"));
        }
    }
}