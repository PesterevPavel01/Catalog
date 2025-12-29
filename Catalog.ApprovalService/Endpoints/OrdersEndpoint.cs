using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ApprovalService.Application.Configurations;
using Catalog.ApprovalService.Application.Processors;
using Catalog.Contracts.Dto.Approval;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.Approval;
using Catalog.Contracts.Events.ApprovalEvents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rebus.Bus;

namespace Catalog.ApprovalService.Endpoints
{
    public class OrdersEndpoint : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.OrdersEndpoints();
    }

    internal static class OrdersEndpointDefinitionExtensions
    {
        public static async Task OrdersEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapGet("{orderCode}", async (
                [FromRoute] string orderCode,
                GetWorkflowsProcessor processor,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(orderCode, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderWorkflowsEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить данные о состоянии заказа"
            });

            group.MapPost("approve", async (
                [FromBody] OrderDto model,
                IBus bus,
                IOptions <ApplicationConfiguration> applicationConfiguration,
                ApproveProcessor processor,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(model.Code, model.UserName, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                if(result.Result.IsCompleted)
                    await bus.Publish(new WorkflowsCancelledEvent(result.Result));

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ApproveOrderEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Согласовать"
            });

            group.MapPost("reject", async (
                [FromBody] OrderDto model,
                IBus bus,
                IOptions<ApplicationConfiguration> applicationConfiguration,
                RejectProcessor processor,
                CancellationToken cancellationToken) =>
                {
                    var result = await processor.ProcessAsync(model, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Publish(new WorkflowRejectedEvent(result.Result));

                    return Results.Ok(result.Result);
                })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("RejectOrderEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Отказать в согласовании"
            });

            group.MapPost("remove", async (
                [FromBody] OrderDto model,
                IBus bus,
                IOptions<ApplicationConfiguration> applicationConfiguration,
                RemoveOrderWorkflowsProcessor processor,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(model.Code, model.UserName, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                await bus.Publish(new OrderApprovalWorkflowsRemoveEvent(result.Result));

                return Results.Ok(result.Result);
            })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderApprovalWorkflowsDisabledEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Удалить все процессы согласования заказа"
            });

            group.MapPost("permission", async (
                [FromBody] ApproveDto model,
                IOptions<ApplicationConfiguration> applicationConfiguration,
                PermissionCheckerProcessor processor,
                CancellationToken cancellationToken) =>
                {
                    var result = await processor.ProcessAsync(model.WorkflowCode, model.UserName, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("PermissionEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Проверить наличие прав"
            });

        }
    }
}