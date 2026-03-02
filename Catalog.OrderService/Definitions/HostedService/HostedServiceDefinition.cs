using Calabonga.AspNetCore.AppDefinitions;
using Catalog.OrderService.Application.HostedService;

namespace Catalog.OrderService.Definitions.HostedService
{
    public class HostedServiceDefinition : AppDefinition
    {
        /// <summary>
        /// Configure services for current application
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddHostedService<OutboxProcessorExecutorHostedService>();
            builder.Services.AddHostedService<OutboxCleanerProcessorExecutorHostedService>();
        }
    }
}