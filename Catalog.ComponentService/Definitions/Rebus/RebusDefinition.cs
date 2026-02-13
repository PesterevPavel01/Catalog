using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ComponentService.Application.Commands;
using Catalog.Contracts.Commands.Exchange;
using Catalog.Contracts.Entities.Rabbit;
using Catalog.Contracts.Interfaces;
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
                .Transport(x => x.UseRabbitMq(rabbitSettings.RabbitUrl, nameof(IComponentQueueEvent)))
                .Routing(r => r.TypeBased()
                    .Map<SetComponentsInCacheCommand>(nameof(IComponentQueueEvent)))
                .Options(x =>
                {
                    x.SetNumberOfWorkers(1);//кол-во потоков
                    x.SetBusName("ComponentService");
                });

                return config;
            }, onCreated: async bus => 
            {
                await bus.Subscribe<ComponentSyncCommand>();
            });

            builder.Services.AutoRegisterHandlersFromAssemblyOf<ExchangeAssemblyReference>();
        }
    }
}
