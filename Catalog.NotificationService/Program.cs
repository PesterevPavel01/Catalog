using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Infrastructure;
using Catalog.NotificationService.Definitions.Configurations;
using Serilog;
using TelegramService.DependencyInjection;
using Catalog.Logging.Middleware;

try
{
    // created builder
    var builder = WebApplication.CreateBuilder(args);

    builder.AddApplicationConfiguration();

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