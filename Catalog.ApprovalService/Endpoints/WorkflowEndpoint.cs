using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.ApprovalService.Application.Services;
using Catalog.Contracts.Events.ApprovalEvents;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rebus.Bus;

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
                Summary = "Получить конфигурацию."
            });

            group.MapPost("create", async (
                IMediator mediator,
                IBus bus,
                HttpContext context,
                IOptions <ApplicationConfiguration> applicationConfiguration,
                [FromBody] string orderCode,
                OrderApprovalInitiatorService approvalInitiatorService) =>
            {
                var result = await approvalInitiatorService.InitializeAsync(orderCode, context.RequestAborted);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                if (result.Ok)
                    await bus.Publish(new WorkflowCreatedEvent(result.Result));

                if (result.Result.IsApprovalCompleted)
                    await mediator.Publish(new WorkflowCompleteCommand(result.Result), context.RequestAborted);

                return Results.Ok(true);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("CreateWorkflowEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Запустить процесс согласования нового проекта."
            });


        }
    }
}