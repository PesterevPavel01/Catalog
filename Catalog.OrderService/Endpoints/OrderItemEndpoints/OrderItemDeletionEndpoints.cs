using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Messages.OrderItemMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.OrderService.Endpoints.OrderItemEndpoints
{
    public class OrderItemDeletionEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderItemDeletionEndpoints();
    }

    internal static class OrderItemDeletionEndpointDefinitionExtensions
    {
        public static async Task MapOrderItemDeletionEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(3, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(3, 0))
                .WithTags($"{nameof(OrderItem)} commands"); ;

            group.MapDelete("{orderCode}/remove/{moduleCode}",
                async (
                    [FromRoute] string orderCode,
                    [FromRoute] string moduleCode,
                    IMediator mediator,
                    HttpContext context) =>
                {
                    var result = await mediator.Send(new RemoveOrderItem.Request(orderCode, moduleCode), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderItemRemoveEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Удалить элемент заказа"
            });
        }
    }
}