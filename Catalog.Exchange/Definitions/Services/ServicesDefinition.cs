using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.ExchangeService.Application.Processors;

namespace Catalog.ExchangeService.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<AuthentificationProcessor>();
            builder.Services.AddScoped<ComponentCreatorProcessor>();
            builder.Services.AddScoped<ComponentLoaderProcessor>();
            builder.Services.AddScoped<ComponentAddNumericParameterProcessor>();
            builder.Services.AddScoped<ComponentAddTextParameterProcessor>();
        }
    }
}
