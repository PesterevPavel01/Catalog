using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Configurations.Rabbit;
using Catalog.Contracts.Events;
using Catalog.Contracts.Events.ApprovalEvents;
using Catalog.Contracts.Interfaces;
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
            var connectionString = builder.Configuration.GetConnectionString("AppDbConnectionString");

            builder.Services.AddRebus(configure: config =>
            {
                config
                .Logging(x => x.Serilog(Log.Logger))
                .Serialization(x => x.UseSystemTextJson())
                .Timeouts(x => x.StoreInMySql(connectionString, $"{nameof(IApprovalQueueEvent)}_rebus_timeouts"))
                .Transport(x => x.UseRabbitMq(rabbitSettings.RabbitUrl, nameof(IApprovalQueueEvent)))
                .Options(x =>
                {
                    x.SetNumberOfWorkers(3);//кол-во потоков
                    x.SetBusName("ApprovalService");
                });

                return config;
            }, onCreated: async bus =>
                {
                    await bus.Subscribe<ModuleChangedEvent>();
                }
            );

            builder.Services.AutoRegisterHandlersFromAssemblyOf<ApprovalAssemblyReference>();
        }
    }
}
