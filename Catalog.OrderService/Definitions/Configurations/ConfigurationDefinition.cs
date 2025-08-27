using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Rabbit;
using Catalog.OrderService.Application.Configurations;

namespace Catalog.OrderService.Definitions.Configurations
{
    public static class ConfigurationDefinition
    {
        public static void AddApplicationConfiguration(this WebApplicationBuilder builder)
        {
            ConfigureAuthorization(builder);

            ConfigureApplication(builder);

            ConfigureRabbit(builder);
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

        private static void ConfigureApplication(WebApplicationBuilder builder)
        {
            builder.Services.Configure<ApplicationConfiguration>(
                builder.Configuration.GetSection("ApplicationConfiguration"));
        }
    }
}