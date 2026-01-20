using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.OrderService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

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
                    //В последних версиях заблокированна возможность добавлять новые элементы в заказ, который уже отправлен на согласование!!!
                    //await bus.Publish(new OrderItemsIncludedEvent(result.Result));

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
