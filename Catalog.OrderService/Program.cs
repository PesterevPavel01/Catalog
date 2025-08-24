using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ComponentCompatibilityValidator.Extension;
using Catalog.ModuleCompositionValidator.Extension;
using Catalog.Web.Definitions.Configurations;
using Serilog;
using Serilog.Events;
using TelegramService.DependencyInjection;

try
{
    // configure logger (Serilog)
    Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

    // created builder
    var builder = WebApplication.CreateBuilder(args);

    builder.AddApplicationConfiguration();

    builder.Services.AddComponentCompabilityValidator();

    builder.Services.AddModuleCompositionValidator();

    builder.Services.AddTelegramService();

    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.AddDefinitions(typeof(Program));

    // create application
    var app = builder.Build();

    // using definition for application
    app.UseDefinitions();

    // using Serilog request logging
    app.UseSerilogRequestLogging();


    app.UseAuthentication();

    app.UseAuthorization();

    app.UseHttpsRedirection();

    // start application
    app.Run();

    return 0;
}
catch (Exception ex)
{
    //var type = ex.GetType().Name;
    //if (type.Equals("HostAbortedException", StringComparison.Ordinal))
    //{
    //    throw;
    //}

    Log.Fatal(ex, "Unhandled exception");
    throw;
}
finally
{
    Log.CloseAndFlush();
}