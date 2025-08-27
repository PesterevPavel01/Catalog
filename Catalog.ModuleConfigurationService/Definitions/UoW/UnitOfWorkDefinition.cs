using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.Infrastructure;

namespace Catalog.ModuleConfigurationService.Definitions.UoW
{
    public class UnitOfWorkDefinition : AppDefinition
    {
        /// <summary>
        /// Configure services for current application
        /// </summary>
        /// <param name="builder"></param>
        public override void ConfigureServices(WebApplicationBuilder builder)
            => builder.Services.AddUnitOfWork<ApplicationDbContext>();
    }
}