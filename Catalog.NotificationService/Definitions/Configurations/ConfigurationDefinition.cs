using Catalog.Contracts.Configurations.Rabbit;
using Catalog.Contracts.Entities.Configurations;
using Catalog.NotificationService.Application.Configurations;
using TelegramService.Configurations;

namespace Catalog.NotificationService.Definitions.Configurations
{
    public static class ConfigurationDefinition
    {
        public static void AddApplicationConfiguration(this WebApplicationBuilder builder)
        {
            ConfigureApplication(builder);

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
        private static void ConfigureApplication(WebApplicationBuilder builder)
        {
            builder.Services.Configure<ApplicationConfiguration>(
                builder.Configuration.GetSection("ApplicationConfiguration"));

            builder.Services.Configure<TelegramBotConfiguration>(
                builder.Configuration.GetSection("TelegramBot"));
        }
    }
}