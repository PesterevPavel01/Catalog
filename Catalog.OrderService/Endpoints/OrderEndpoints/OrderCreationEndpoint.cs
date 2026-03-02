using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Messages.OrderItemMessages;
using Catalog.OrderService.Application.Messages.OrderMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.OrderService.Endpoints.OrderEndpoints;

public class OrderCreationEndpoint: AppDefinition
{
    public override void ConfigureApplication(WebApplication app)
        => app.MapOrderCreationEndpoints();
}

internal static class OrderCreationEndpointDefinitionExtensions
{
    public static async Task MapOrderCreationEndpoints(this IEndpointRouteBuilder routes)
    {
        var versionSet = routes.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();

        var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(2, 0))
            .WithTags($"{nameof(Order)} commands");

        group.MapPost("create",
            async (
                [FromBody] CreateOrderDto model,
                IMediator mediator,
                IBus bus,
                HttpContext context) =>
            {
                var result = await mediator.Send(new CreateOrder.Request(model), context.RequestAborted);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                if (model.OrderItems.Any())
                {
                    var operationResult = await mediator.Send(new CreateOrderItem.Request(model.OrderItems), context.RequestAborted);

                    if (!operationResult.Ok)
                        return Results.BadRequest(operationResult.Error);
                }

                return Results.Ok(result.Result);
            })
        .Produces(200)
        .ProducesProblem(401)
        .WithName("OrderCreateEndpoint")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Создание нового заказа.",
        });
    }
}
