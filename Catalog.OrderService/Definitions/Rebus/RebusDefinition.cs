using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Entities.Rabbit;
using Catalog.Contracts.Events.Approval;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Response;
using Catalog.OrderService.Application.Commands;
using Rebus.Config;
using Rebus.Routing.TypeBased;
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
                .Transport(x => x.UseRabbitMq(rabbitSettings.RabbitUrl, nameof(IOrderQueueEvent)))
                .Routing(r => r.TypeBased()
                    .Map<SetOrderEventsInCacheCommand>(nameof(IOrderQueueEvent))
                    .Map<ModuleChangePermissionResponse>(nameof(IModuleQueueEvent)))
                .Options(x =>
                {
                    x.EnableSynchronousRequestReply();
                    x.SetNumberOfWorkers(5);//кол-во потоков
                    x.SetBusName("OrderService");
                });

                return config;
            }
            , onCreated: async bus =>{
                await bus.Subscribe<CreateOrderEventCommand>();
                await bus.Subscribe<WorkflowCreatedEvent>();
                await bus.Subscribe<WorkflowsCancelledEvent>();
                await bus.Subscribe<WorkflowRejectedEvent>();
                await bus.Subscribe<OrderApprovalWorkflowsRemoveEvent>();
                await bus.Subscribe<OrderAddMessageEvent>();
                await bus.Subscribe<OrderDisabledEvent>();
            }
            );

            builder.Services.AutoRegisterHandlersFromAssemblyOf<OrderAssemblyReference>();
        }
    }
}
