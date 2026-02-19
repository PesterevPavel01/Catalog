using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Messages.OrderItemMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.OrderService.Endpoints.OrderItemEndpoints
{
    public class OrderItemUpdateEndpoint : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderItemUpdateEndpoints();
    }

    internal static class OrderItemUpdateEndpointDefinitionExtensions
    {
        public static async Task MapOrderItemUpdateEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(3, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(3, 0))
                .WithTags($"{nameof(OrderItem)} commands");

            group.MapPatch("set-quantity",
                async (
                    [FromBody] CreateOrderItemDto orderItem,
                    IMediator mediator,
                    IBus bus,
                    HttpContext context) =>
                {
                    var result = await mediator.Send(new SetQuantity.Request(orderItem), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Send(new OrderChangedEvent(orderItem.OrderCode));

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderItemSetQuantityEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Установить количество модулей."
            });
        }
    }
}