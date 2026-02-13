using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.ComponentService.Application.Managers;
using Catalog.ComponentService.Application.Processors;
using Catalog.ExchangeService.Application.Processors;

namespace Catalog.ExchangeService.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ComponentCreateProcessor>();
            builder.Services.AddScoped<ComponentUpdateProcessor>();
            builder.Services.AddScoped<ComponentLoaderProcessor>();
            builder.Services.AddScoped<ComponentAddNumericParameterProcessor>();
            builder.Services.AddScoped<ComponentAddTextParameterProcessor>();
            builder.Services.AddScoped<ComponentRemovalProcessor>();
            builder.Services.AddScoped<DisableComponentProcessor>();
        
            builder.Services.AddScoped<TextParametersReplacer>();

            builder.Services.AddScoped<CachedComponentLoaderProcessor>();

            builder.Services.AddScoped<ComponentSyncManager>();

            builder.Services.AddScoped<AuthenticationProcessor>();
        }
    }
}
