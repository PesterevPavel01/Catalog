using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Configurations.Rabbit;
using Catalog.Contracts.Events.CustomerEvents;
using Catalog.Contracts.Events.ExchangeEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Rebus.Config;
using Rebus.Serialization.Json;
using Serilog;

namespace Catalog.NotificationService.Definitions.Rebus
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
                .Transport(x => x.UseRabbitMq(rabbitSettings.RabbitUrl, nameof(INotificationQueueEvent)))
                .Options(x =>
                {
                    x.SetNumberOfWorkers(3);//кол-во потоков
                    x.SetBusName("NotificationService");
                });

                return config;
            }, onCreated: async bus =>
                {
                    await bus.Subscribe<MarkOrdersAsProducedEvent>();
                    await bus.Subscribe<OrderCompletedEvent>();
                    await bus.Subscribe<CustomerCreatedEvent>();
                    await bus.Subscribe<OrderWorkflowCreatedEvent>();
                    await bus.Subscribe<OrderWorkflowsCompletedEvent>();
                    await bus.Subscribe<OrderRejectedEvent>();
                    await bus.Subscribe<OrderRejectedFromProductionEvent>();
                    await bus.Subscribe<OrderCancelledEvent>();
                    await bus.Subscribe<CustomModuleChangedEvent>();
                    await bus.Subscribe<OrderAddMessageEvent>();
                    await bus.Subscribe<OrderDisabledEvent>();
                    await bus.Subscribe<OrderExportedEvent>();
                    await bus.Subscribe<SyncFailedEvent>();
                    
                }
            );
            builder.Services.AutoRegisterHandlersFromAssemblyOf<NotificationAssemblyReference>();
        }
    }
}
