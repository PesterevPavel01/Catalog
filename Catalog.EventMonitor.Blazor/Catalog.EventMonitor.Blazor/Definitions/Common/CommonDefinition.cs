namespace Catalog.EventMonitor.Blazor.Definitions.Common;
using Calabonga.Blazor.AppDefinitions;
using Catalog.EventMonitor.Blazor.Components;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;
using System.Reflection;


public class CommonDefinition : AppDefinition
{
    public override void ConfigureServices(WebApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();
    }

    public override void ConfigureApplication(WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies([.. ModuleDefinitions.Instance.Assemblies]);
    }
}

