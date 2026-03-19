using Calabonga.Blazor.AppDefinitions;
using Catalog.Contracts.Interfaces;
using Catalog.ModuleOrderEvents.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.ModuleOrderEvents;

public class ModuleOrderEventsDefinition:AppDefinition
{
    public override bool Exported => true;

    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBlazorModule, OrderEventsBlazorModule>();
        builder.Services.AddSingleton<IEventStoreService, EventStoreService>();
    }
}

