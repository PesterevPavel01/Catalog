using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Message;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Messages.OrderMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.OrderService.Endpoints.OrderEndpoints;

public class OrderUpdateEndpoint: AppDefinition
{
    public override void ConfigureApplication(WebApplication app)
        => app.MapOrderUpdateEndpoints();
}

internal static class OrderUpdateEndpointDefinitionExtensions
{
    public static async Task MapOrderUpdateEndpoints(this IEndpointRouteBuilder routes)
    {
        var versionSet = routes.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();

        var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(new ApiVersion(2, 0))
            .WithTags($"{nameof(Order)} commands");

        group.MapPatch("add-message",
                async (
                    [FromBody] CreateMessageDto model,
                    IMediator mediator,
                    HttpContext context) =>
                {
                    var result = await mediator.Send(new AddMessage.Request(model), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderAddMessageEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Добавить комментарий к элементу заказа."
            });

        group.MapPatch("produced",
                async (
                    [FromBody] IEnumerable<string> codes,
                    IMediator mediator,
                    HttpContext context) =>
                {
                    var result = await mediator.Send(new CompleteProduction.Request(codes), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok();
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ProducedOrdersEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Установить заказам статус \"Производство завершено\"."
            });
    }
}
