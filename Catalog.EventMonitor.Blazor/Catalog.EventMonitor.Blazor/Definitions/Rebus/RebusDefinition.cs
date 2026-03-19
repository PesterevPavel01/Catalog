using Calabonga.Blazor.AppDefinitions;
using Catalog.Contracts.Configurations.Rabbit;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Rebus.Config;
using Rebus.Serialization.Json;
using Serilog;

namespace Catalog.EventMonitor.Blazor.Definitions.Rebus;

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
            .Transport(x => x.UseRabbitMq(rabbitSettings.RabbitUrl, nameof(IEventMonitorQueueEvent)))
            .Options(x =>
            {
                x.SetNumberOfWorkers(3);//кол-во потоков
                x.SetBusName("EventMonitor");
            });

            return config;
        }, onCreated: async bus =>
        {

            await bus.Subscribe<MarkOrdersAsProducedEvent>();
            await bus.Subscribe<OrderAddMessageEvent>();
            await bus.Subscribe<OrderCancelledEvent>();
            await bus.Subscribe<OrderCompletedEvent>();
            await bus.Subscribe<OrderDisabledEvent>();
            await bus.Subscribe<OrderExportedEvent>();
            await bus.Subscribe<OrderModuleChangedEvent>();
            await bus.Subscribe<OrderRejectedEvent>();
            await bus.Subscribe<OrderRejectedFromProductionEvent>();
            await bus.Subscribe<OrderWorkflowCreatedEvent>();
            await bus.Subscribe<OrderWorkflowsCompletedEvent>();
        }
        );
        builder.Services.AutoRegisterHandlersFromAssemblyOf<EventMonitorAssemblyReference>();
    }
}