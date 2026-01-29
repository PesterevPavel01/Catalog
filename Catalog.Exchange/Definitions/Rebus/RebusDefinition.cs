using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Entities.Rabbit;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Request;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Serialization.Json;
using Serilog;

namespace Catalog.ExchangeService.Definitions.Rebus
{
    public class RebusDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            var rabbitSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitSettings>();
            var connectionString = builder.Configuration.GetConnectionString("AppDbConnectionString");

            builder.Services.AddRebus(configure: config =>
            {
                config
                .Logging(x => x.Serilog(Log.Logger))
                .Serialization(x => x.UseSystemTextJson())
                .Transport(x => x.UseRabbitMq(rabbitSettings.RabbitUrl, nameof(IExchangeQueueEvent)))
                .Timeouts(x => x.StoreInMySql(connectionString, $"{nameof(IExchangeQueueEvent)}_rebus_timeouts"))
                .Routing(r => r.TypeBased()
                    .Map<LatestChangesOrdersRequest>(nameof(IOrderQueueEvent)))
                .Options(x =>
                {
                    x.EnableSynchronousRequestReply();
                    x.SetNumberOfWorkers(1);
                    x.SetBusName("ExchangeService");
                });

                return config;
            }, onCreated: async bus =>
                { }
            );

            builder.Services.AutoRegisterHandlersFromAssemblyOf<ExchangeAssemblyReference>();
        }
    }
}
