using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors;
using Catalog.Application.Processors.AuthorizationProcessor;

namespace Catalog.Exchange.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<AuthentificationProcessor>();
            builder.Services.AddScoped<ModuleCreatorProcessor>();
        }
    }
}
