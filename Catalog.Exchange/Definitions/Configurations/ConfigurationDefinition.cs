using Catalog.Contracts.Entities.Rabbit;
using Catalog.Domain.Entities.Authorization;

namespace Catalog.Exchange.Definitions.Configurations
{
    public static class ConfigurationDefinition
    {
        /// <summary>
        /// Добавляет общую конфигурацию из корневого appsettings.json
        /// и настраивает секцию Authorization
        /// </summary>
        public static void AddSharedConfiguration(this WebApplicationBuilder builder)
        {
            LoadSharedSettings(builder);

            ConfigureAuthorization(builder);

            ConfigureRabbit(builder);
        }

        private static void LoadSharedSettings(WebApplicationBuilder builder)
        {
            var solutionRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));
            var sharedConfigPath = Path.Combine(solutionRoot, "appsettings.json");

            Console.WriteLine($"Загружаем общий конфиг из: {sharedConfigPath}");

            builder.Configuration
                .AddJsonFile(sharedConfigPath, optional: false, reloadOnChange: true)
                .AddJsonFile(
                    Path.Combine(solutionRoot, $"appsettings.{builder.Environment.EnvironmentName}.json"),
                    optional: true);
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