using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.Contracts;
using Catalog.Infrastructure;
using Catalog.OrderService.Application.Handlers.QueryHandlers;

namespace Catalog.Web.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<AuthenticationProcessor>();

            builder.Services.AddScoped<OrdersQueryHandler>();

            builder.Services.AddScoped<OrderQueryHandler>();
            
            builder.Services.AddScoped<CachedOrdersQueryHandler>();

            builder.Services.AddScoped<LastModifiedOrdersQueryHandler>();

            builder.Services.AddScoped<IOutboxProcessor, OutboxProcessor>();

            builder.Services.AddScoped<IOutboxCleanerProcessor, OutboxCleanerProcessor>();
        }
    }
}
