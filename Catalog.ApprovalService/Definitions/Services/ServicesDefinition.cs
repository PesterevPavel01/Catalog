using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.ApprovalService.Application.Processors;

namespace Catalog.ApprovalService.Definitions.Services
{
    public class ServicesDefinition : AppDefinition
    {
        public override void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<AuthenticationProcessor>();

            builder.Services.AddScoped<CreateApprovalStageProcessor>();

            builder.Services.AddScoped<GetWorkflowsProcessor>();

            builder.Services.AddScoped<ApproveProcessor>();

            builder.Services.AddScoped<RejectProcessor>();

            builder.Services.AddScoped<PermissionCheckerProcessor>();

            builder.Services.AddScoped<ApprovalWorkflowInitiatorProcessor>();
        }
    }
}
