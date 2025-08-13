using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application;
using Catalog.Application.Processors.AuthorizationProcessor;

namespace Catalog.Web.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ComponentServices>();
            builder.Services.AddScoped<AuthentificationProcessor>();
        }
    }
}
