using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Entities.Rabbit;
using Catalog.Contracts.Events.OrderEvents;
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

            builder.Services.AddRebus(configure: config =>
            {
                config
                .Logging(x => x.Serilog(Log.Logger))
                .Serialization(x => x.UseSystemTextJson())
                .Transport(x => x.UseRabbitMq(rabbitSettings.RabbitUrl, nameof(IExchangeQueueEvent)))
                .Routing(r => r.TypeBased()
                    .Map<LatestChangesOrdersRequest>(nameof(IOrderQueueEvent)))
                .Options(x =>
                {
                    x.EnableSynchronousRequestReply();
                    x.SetNumberOfWorkers(3);//кол-во потоков
                    x.SetBusName("ExchangeService");
                });

                return config;
            }, onCreated: async bus =>
                {
                    await bus.Subscribe<OrderCreatedEvent>();
                }
            );

            builder.Services.AutoRegisterHandlersFromAssemblyOf<ExchangeAssemblyReference>();
        }
    }
}
