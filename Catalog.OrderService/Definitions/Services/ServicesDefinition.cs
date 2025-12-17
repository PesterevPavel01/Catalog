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

            builder.Services.AddScoped<OrderCreatorProcessor>();
            builder.Services.AddScoped<OrdersQueryHandler>();
            builder.Services.AddScoped<OrderDisableProcessor>();
            builder.Services.AddScoped<OrderItemRemovalProcessor>();
            builder.Services.AddScoped<OrderItemCreatorProcessor>();
            builder.Services.AddScoped<CleanupOldOrderCommandHandler>();

            builder.Services.AddScoped<OrderMessagesQueryHandler>();
            builder.Services.AddScoped<SetOrderItemQuantityCommandHandler>();
            builder.Services.AddScoped<AddMessageCommandHandler>();
        }
    }
}
