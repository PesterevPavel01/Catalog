using Catalog.Contracts.Configurations;
using Catalog.Contracts.Configurations.Rabbit;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Redis.Configuration;
using TelegramService.Configurations;

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
            builder.Services.Configure<OrderConfiguration>(
                builder.Configuration.GetSection("ApplicationConfiguration"));

            builder.Services.Configure<TelegramBotConfiguration>(
                builder.Configuration.GetSection("TelegramBot"));

            builder.Services.Configure<RedisConfiguration>(
                builder.Configuration.GetSection("RedisConfiguration"));
        }
    }
}