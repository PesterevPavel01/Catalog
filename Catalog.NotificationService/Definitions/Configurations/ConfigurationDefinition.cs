using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Rabbit;

namespace Catalog.NotificationService.Definitions.Configurations
{
    public static class ConfigurationDefinition
    {
        public static void AddSharedConfiguration(this WebApplicationBuilder builder)
        {
            ConfigureAuthorization(builder);

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
    }
}