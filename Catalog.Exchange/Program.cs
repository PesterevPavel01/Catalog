using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ComponentParametersValidator.Extensions;
using Catalog.ExchangeService.Definitions.Configurations;
using Catalog.Infrastructure;
using Catalog.Logging.Middleware;
using Catalog.Redis.Extension;
using Rebus.Config;
using Serilog;
using TelegramService.DependencyInjection;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddApplicationConfiguration();

    builder.Services.AddComponentParametersValidator();

    builder.Services.AddTelegramService();

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