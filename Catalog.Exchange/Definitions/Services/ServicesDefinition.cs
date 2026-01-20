using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.ExchangeService.Application.Handlers.Orders;
using Catalog.ExchangeService.Application.Processors;

namespace Catalog.ExchangeService.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<RegistrationProcessor>();
            builder.Services.AddScoped<AuthenticationProcessor>();

            builder.Services.AddScoped<UserGetExternalIdProcessor>();
            builder.Services.AddScoped<UserSetRoleProcessor>();
            builder.Services.AddScoped<UserRolesLoaderProcessor>();
            builder.Services.AddScoped<UnassignedUserLoaderProcessor>();
            builder.Services.AddScoped<UserDisableProcessor>();

            builder.Services.AddScoped<GetLatestChangesOrdersCommandHandler>();
            builder.Services.AddScoped<ConfirmOrderSyncCommandHandler>();
        }
    }
}
