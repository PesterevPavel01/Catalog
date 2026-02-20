using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.OrderService.Application.Messages.OrderEventMessages;
using Catalog.OrderService.Application.Messages.OrderItemMessages;

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
        }
    }
}
