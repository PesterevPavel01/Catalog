using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Infrastructure;
using Catalog.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Catalog.OrderService.Definitions.DbContext
{
    public class DbContextDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ConvertDomainEventToOutboxMessageInterceptor>();

            var connectionString = builder.Configuration.GetConnectionString("AppDbConnectionString");

            builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .AddInterceptors(serviceProvider.GetRequiredService<ConvertDomainEventToOutboxMessageInterceptor>());
            });
        }
    }
}