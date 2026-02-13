using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Exchange;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Handlers.Orders;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.ExchangeService.Endpoints
{
    public class OrderSyncEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderSyncEndpoints();
    }
    internal static class OrderSyncEndpointDefinitionExtensions
    {
        public static async Task MapOrderSyncEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes
                .NewApiVersionSet()
                .HasApiVersion(new ApiVersion(3, 0))
                .ReportApiVersions()
                .Build();

            var group = routes
                .MapGroup("/api/v{version:apiVersion}/orders/sync/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(3, 0))
                .WithTags(nameof(Order)); ;

            group
                .MapGet("", 
                    async(
                        GetLatestChangesOrdersCommandHandler handler,
                        CancellationToken cancellationToken) =>
                    {
                        var orders = await handler.HandleAsync("Обмен заказами с 1с", cancellationToken);

                        if (!orders.Ok)
                            Results.BadRequest(orders.Error);

                        return Results.Ok(orders.Result);
                    })
                .RequireAuthorization("Administrator")
                .Produces(200)
                .ProducesProblem(401)
                .WithName("OrderSyncEndpoint")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Получить последние изменения"
                });

            group
                .MapPost("confirm",
                    async (
                        [FromBody] SyncConfirmationDto syncResult,
                        ConfirmOrderSyncCommandHandler handler, 
                        CancellationToken cancellationToken) =>
                    {
                        var result = await handler.HandleAsync(syncResult, cancellationToken);

                        if (!result.Ok)
                            return Results.BadRequest(result.Error);

                        return Results.Ok(result.Result);
                    })
                .RequireAuthorization("Administrator")
                .Produces(200)
                .ProducesProblem(401)
                .WithName("ConfirmOrderSyncEndpoint")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Подтвердить успешный обмен"
                });
        }
    }


}