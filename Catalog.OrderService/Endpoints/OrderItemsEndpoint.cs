using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.OrderService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.OrderService.Endpoints
{
    public class OrderItemsEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderItemsEndpoints();
    }

    internal static class OrderItemsEndpointDefinitionExtensions
    {
        public static async Task MapOrderItemsEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(3, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(3, 0));

            group.MapPost("order-item/create", 
                async (
                    [FromBody] IEnumerable<CreateOrderItemDto> models,
                    IBus bus,
                    OrderItemCreatorProcessor orderProcessor,
                    CancellationToken cancellationToken) =>
                {
                    var result = await orderProcessor.ProcessAsync(models, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Publish(new OrderItemsIncludedEvent(models));

                    return Results.Ok(result.Result);
                })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderItemCreateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создание новых элементов заказа.",
            });

            group.MapDelete("{orderCode}/remove/{moduleCode}", 
                async (
                    [FromRoute] string orderCode,
                    [FromRoute] string moduleCode,
                    OrderItemRemovalProcessor processor,
                    CancellationToken cancellationToken) =>
                {
                    var result = await processor.ProcessAsync(orderCode, moduleCode, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
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
