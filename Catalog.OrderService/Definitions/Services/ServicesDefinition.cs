using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.OrderService.Application.Processors;

namespace Catalog.Web.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<OrderCreatorProcessor>();
            builder.Services.AddScoped<OrderLoaderProcessor>();
            builder.Services.AddScoped<AuthenticationProcessor>();
            builder.Services.AddScoped<OrdersByCustomerLoginProcessor>();
            builder.Services.AddScoped<OrderDisableProcessor>();
            builder.Services.AddScoped<OrderItemRemovalProcessor>();
            builder.Services.AddScoped<OrderItemCreatorProcessor>();
        }
    }
}
