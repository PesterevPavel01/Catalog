using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ApprovalService.Application.Configurations;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Endpoints
{
    public class WorkflowEndpoint : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.WorkflowEndpoints();
    }

    internal static class WorkflowEndpointDefinitionExtensions
    {
        public static async Task WorkflowEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/workflows/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapGet("map", async (
                IOptions<ApplicationConfiguration> applicationConfiguration,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok(applicationConfiguration.Value.ApprovalWorkflowMap);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetWorkflowMapEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить конфигурацию"
            });
        }
    }
}