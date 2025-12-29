using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.ApprovalService.Application.Processors;
using Catalog.ApprovalService.Application.Processors.OrderItems;
using Catalog.ApprovalService.Application.Services;

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

            builder.Services.AddScoped<RemoveOrderWorkflowsProcessor>(); 

            builder.Services.AddScoped<PermissionCheckerProcessor>();

            builder.Services.AddScoped<OrderApprovalInitiatorService>();

            builder.Services.AddScoped<ModuleApprovalWorkflowRestartProcessor>();

            builder.Services.AddScoped<OrderItemApprovalWorkflowCreatorProcessor>();

            builder.Services.AddScoped<OrderItemApprovalInitiatorService>();
        }
    }
}
