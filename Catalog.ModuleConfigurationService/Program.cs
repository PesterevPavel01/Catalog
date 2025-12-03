using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ComponentCompatibilityValidator.Extension;
using Catalog.Infrastructure;
using Catalog.ModuleCompositionValidator.Extension;
using Catalog.ModuleConfigurationService.Definitions.Configurations;
using Catalog.ModuleParametersValidator.Extensions;
using Serilog;
using Catalog.Logging.Middleware;
using TelegramService.DependencyInjection;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddApplicationConfiguration();

    builder.Services.AddModuleParametersValidator();

    builder.Services.AddComponentCompatibilityValidator();

    builder.Services.AddModuleCompositionValidator();

    builder.Services.AddTelegramService();

    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.AddDefinitions(typeof(Program));

    var app = builder.Build();

    await DatabaseInitializer.InitializeAsync(app.Services);

    app.UseDefinitions();

    app.UseRequestResponseLogging();

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseHttpsRedirection();

    app.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
    throw;
}
finally
{
    Log.CloseAndFlush();
}