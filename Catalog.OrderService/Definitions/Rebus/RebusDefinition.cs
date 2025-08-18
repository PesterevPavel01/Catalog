using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Entities.Rabbit;
using Catalog.Contracts.Events;
using Catalog.Contracts.Interfaces;
using Rebus.Config;
using Rebus.Serialization.Json;
using Serilog;

namespace Catalog.OrderService.Definitions.Rebus
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
                .Transport(x => x.UseRabbitMq(rabbitSettings.RabbitUrl, "IOrderQueueEvent"))
                .Options(x =>
                {
                    x.SetNumberOfWorkers(5);//кол-во потоков
                    x.SetBusName("OrderService");
                });

                return config;
            }
            , onCreated: async bus =>
            {
                await bus.Subscribe<OrderCreatedEvent>();
            }
            );

            builder.Services.AutoRegisterHandlersFromAssemblyOf<OrderAssemblyReference>();
        }
    }
}
