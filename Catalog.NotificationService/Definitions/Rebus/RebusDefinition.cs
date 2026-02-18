using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Commands.Exchange;
using Catalog.Contracts.Entities.Rabbit;
using Catalog.Contracts.Events;
using Catalog.Contracts.Events.Approval;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Events.CustomerEvents;
using Catalog.Contracts.Events.ExchangeEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.NotificationService.Definitions;
using Rebus.Config;
using Rebus.Serialization.Json;
using Serilog;

namespace Catalog.ApprovalService.Definitions.Rebus
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
                    await bus.Subscribe<CustomerCreatedEvent>();
                    await bus.Subscribe<WorkflowCreatedEvent>();
                    await bus.Subscribe<WorkflowsCancelledEvent>();
                    await bus.Subscribe<WorkflowRejectedEvent>();
                    await bus.Subscribe<OrderApprovalWorkflowsRemoveEvent>();
                    await bus.Subscribe<CustomWorkflowChangedEvent>();
                    await bus.Subscribe<OrderAddMessageEvent>();
                    await bus.Subscribe<OrderDisabledEvent>();
                    await bus.Subscribe<SyncFailedEvent>();
                    
                }
            );
            builder.Services.AutoRegisterHandlersFromAssemblyOf<NotificationAssemblyReference>();
        }
    }
}
