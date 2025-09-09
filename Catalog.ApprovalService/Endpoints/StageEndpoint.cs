using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.ApprovalService.Application.Processors;
using Catalog.Contracts.Dto.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Catalog.ApprovalService.Endpoints
{
    public class StageEndpoint : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapStageEndpoints();
    }

    internal static class OrderEndpointDefinitionExtensions
    {
        public static async Task MapStageEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/stages/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPut("create", async (
                [FromBody] IEnumerable<SimpleEntityDto> stages,
                IOptions<ApplicationConfiguration> applicationConfiguration,
                CreateApprovalStageProcessor processor,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(stages, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("CreateApprovalStagesEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создать стадию"
            });
        }
    }


}
