using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.OrderService.Application.Handlers.CommandHandlers;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.OrderService.Application.Processors;

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

            builder.Services.AddScoped<OrderItemRemovalProcessor>();
            builder.Services.AddScoped<LatestChangesOrdersQueryHandler>();

            builder.Services.AddScoped<ConstructorOrderEventQueriesHandler>();
            builder.Services.AddScoped<ApplicationUserOrderEventQueriesHandler>();

            builder.Services.AddScoped<SetOrderItemQuantityCommandHandler>();
        }
    }
}
