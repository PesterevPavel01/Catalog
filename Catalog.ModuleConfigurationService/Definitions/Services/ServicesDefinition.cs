using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.ModuleConfigurationService.Application.Processors;

namespace Catalog.ModuleConfigurationService.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ModuleCreatorProcessor>();
            builder.Services.AddScoped<ModuleLoaderProcessor>();
            builder.Services.AddScoped<ModuleComplectationProcessor>(); 
            builder.Services.AddScoped<AuthentificationProcessor>();
        }
    }
}
