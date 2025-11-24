using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.OrderService.Application.Handlers.CommandHandlers;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.OrderService.Endpoints.OrderItemEndpoints
{
    public class OrderItemQueryEndpoint : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderItemQueryEndpoints();
    }

    internal static class OrderItemQueryEndpointDefinitionExtensions
    {
        public static async Task MapOrderItemQueryEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(3, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(3, 0));

            group.MapPatch("set-quantity",
                async (
                    [FromBody] CreateOrderItemDto orderCode,
                    SetOrderItemQuantityCommandHandler commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await commandHandler.ExecuteAsync(orderCode, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

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