using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Entities.Rabbit;
using Catalog.Contracts.Events;
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
                    x.SetNumberOfWorkers(5);//кол-во потоков
                    x.SetBusName("NotificationService");
                });

                return config;
            }, onCreated: async bus =>
                {
                    await bus.Subscribe<OrderCreatedEvent>();
                    await bus.Subscribe<ComponentCreatedEvent>();
                    await bus.Subscribe<ApprovalCompletedEvent>();
                }
            );

            builder.Services.AutoRegisterHandlersFromAssemblyOf<NotificationAssemblyReference>();
        }
    }
}
