using Calabonga.AspNetCore.AppDefinitions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.ComponentService.Definitions.Redis
{
    public class RedisDefinition:AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("Redis");

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = builder.Configuration["RedisCache:InstanceName"] ?? "Catalog_";
            });
        }
    }
}
