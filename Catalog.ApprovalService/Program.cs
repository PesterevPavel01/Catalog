using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ApprovalService.Definitions.Configurations;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Infrastructure;
using Serilog;
using Catalog.Logging.Middleware;
using TelegramService.DependencyInjection;

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.AddApplicationConfiguration();

    builder.Services.AddTelegramService();

    builder.Services.Configure<AuthorizationSettings>(builder.Configuration.GetSection("Authorization"));

    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.AddDefinitions(typeof(Program));

    var app = builder.Build();

    await DatabaseInitializer.InitializeAsync(app.Services);

    await RequiredStageDefinition.CreateAddRequiredStage(app.Services);

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