using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.OrderService.Application.Processors;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.OrderService.Endpoints.OrderEndpoints
{
    public class OrderDeletionEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderDeletionEndpoints();
    }

    internal static class OrderDeletionEndpointDefinitionExtensions
    {
        public static async Task MapOrderDeletionEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPatch("{orderCode}/disable", 
                async (
                    [FromRoute] string orderCode,
                    OrderDisableProcessor processor,
                    CancellationToken cancellationToken) =>
                {
                    var result = await processor.ProcessAsync(orderCode, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderDisableEndpoint")
            .WithOpenApi(operation => new(operation)
            {
            Summary = "Деактивировать заказ"
            });
        }
    }


}
