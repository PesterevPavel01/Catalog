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
            builder.Services.AddScoped<ModuleCreatorProcessor>();
            builder.Services.AddScoped<ModuleComplectationProcessor>(); 
            builder.Services.AddScoped<AuthentificationProcessor>();
        }
    }
}
