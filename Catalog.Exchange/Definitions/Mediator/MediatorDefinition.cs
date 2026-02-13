using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ExchangeService.Definitions.Mediator.Base;
using MediatR;

namespace Catalog.ExchangeService.Definitions.Mediator
{
    public class MediatorDefinition : AppDefinition
    {
        /// <summary>
        /// Configure services for current application
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            //builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkTransactionBehavior<,>));
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        }
    }
}