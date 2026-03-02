using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Messages.OrderItemMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.OrderService.Endpoints.OrderItemEndpoints
{
    public class OrderItemCreationEndpoint : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderItemCreationEndpoints();
    }

    internal static class OrderItemsCreationEndpointDefinitionExtensions
    {
        public static async Task MapOrderItemCreationEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(3, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(3, 0))
                .WithTags($"{nameof(OrderItem)} commands");

            group.MapPost("order-item/create",
                async (
                    [FromBody] IEnumerable<CreateOrderItemDto> models,
                    [FromServices] IMediator mediator,
                    HttpContext context,
                    CancellationToken cancellationToken) =>
                {
                    var result = await mediator.Send(new CreateOrderItem.Request(models), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderItemCreateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создание новых элементов заказа.",
            });
        }
    }
}
