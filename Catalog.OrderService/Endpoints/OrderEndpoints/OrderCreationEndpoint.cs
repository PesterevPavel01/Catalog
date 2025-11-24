using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.OrderService.Application.Configurations;
using Catalog.OrderService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Catalog.OrderService.Endpoints.OrderEndpoints
{
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
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("create",
                async (
                    [FromBody] CreateOrderDto model,
                    IOptions <ApplicationConfiguration> applicationConfiguration,
                    OrderCreatorProcessor orderProcessor,
                    OrderItemCreatorProcessor orderItemProcessor,
                    CancellationToken cancellationToken) =>
                {
                    var result = await orderProcessor.ProcessAsync(model, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    if (model.OrderItems.Any())
                    {
                        var operationResult = await orderItemProcessor.ProcessAsync(model.OrderItems, cancellationToken);

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
}
