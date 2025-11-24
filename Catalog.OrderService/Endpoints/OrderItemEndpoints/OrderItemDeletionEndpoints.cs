using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.OrderService.Application.Processors;
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
                .HasApiVersion(new ApiVersion(3, 0));

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