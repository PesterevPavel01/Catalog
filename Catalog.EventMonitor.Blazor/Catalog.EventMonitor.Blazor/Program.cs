using Calabonga.Blazor.AppDefinitions;
using Catalog.EventMonitor.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddBlazorModulesDefinitions("Modules", typeof(App));

var app = builder.Build();

app.UseDefinitions();

app.Run();
