using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Infrastructure;
using Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Catalog.NotificationService.Definitions.DbContext
{
    public class DbContextDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("AppDbConnectionString");

            builder.Services.AddSingleton<DateInterceptors>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                    .LogTo(Console.WriteLine, LogLevel.Information);
            });
        }
    }
}