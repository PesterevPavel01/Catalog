using Catalog.Contracts.Configurations.Rabbit;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Redis.Configuration;
using TelegramService.Configurations;

namespace Catalog.ExchangeService.Definitions.Configurations
{
    public static class ConfigurationDefinition
    {
        public static void AddApplicationConfiguration(this WebApplicationBuilder builder)
        {
            ConfigureAuthorization(builder);

            ConfigureApplication(builder);

            ConfigureRabbit(builder);
        }

        private static void ConfigureApplication(WebApplicationBuilder builder)
        {
            builder.Services.Configure<ComponentConfiguration>(
                builder.Configuration.GetSection("ApplicationConfiguration"));

            builder.Services.Configure<RedisConfiguration>(
                builder.Configuration.GetSection("RedisConfiguration"));

            builder.Services.Configure<TelegramBotConfiguration>(
                builder.Configuration.GetSection("TelegramBot"));
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