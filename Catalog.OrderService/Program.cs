using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ComponentCompatibilityValidator.Extension;
using Catalog.FacadeOrderTitleValidator.Extensions;
using Catalog.FacadesOrderTitleValidator.Extensions;
using Catalog.Infrastructure;
using Catalog.Logging.Middleware;
using Catalog.ModuleCompositionValidator.Extension;
using Catalog.OrderService.Definitions.Configurations;
using Serilog;
using Catalog.Redis.Extension;
using TelegramService.DependencyInjection;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.AddApplicationConfiguration();

    builder.Services.AddTelegramService();

    builder.Services.AddFacadeOrderTitleValidator();

    builder.Services.AddComponentCompatibilityValidator();

    builder.Services.AddModuleCompositionValidator();

    builder.Services.AddFacadesOrderCompositionValidator();

    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.AddDefinitions(typeof(Program));

    builder.Services.AddRedis();

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