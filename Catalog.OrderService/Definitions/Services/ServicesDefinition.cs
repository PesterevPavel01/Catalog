using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors;
using Catalog.Application.Processors.AuthorizationProcessor;

namespace Catalog.Web.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<OrderCreatorProcessor>();
            builder.Services.AddScoped<AuthentificationProcessor>();
        }
    }
}
