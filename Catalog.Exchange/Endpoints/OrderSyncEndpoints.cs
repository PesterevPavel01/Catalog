using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
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
                .HasApiVersion(new ApiVersion(3, 0));

            group
                .MapGet("", 
                    async(
                        GetLatestChangesOrdersCommandHandler handler,
                        CancellationToken cancellationToken) =>
                    {
                        var orders = await handler.HandleAsync("Обмен с 1с", cancellationToken);

                        if (!orders.Ok)
                            Results.BadRequest(orders.Error);

                        return Results.Ok(orders.Result);
                    })
                //.RequireAuthorization("Administrator")
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
                        [FromBody] string ExchangeEventCode,
                        ConfirmOrderSyncCommandHandler handler, 
                        CancellationToken cancellationToken) =>
                    {
                        var orders = await handler.HandleAsync(ExchangeEventCode, cancellationToken);

                        if (!orders.Ok)
                            Results.BadRequest(orders.Error);

                        return Results.Ok(orders.Result);
                    })
                //.RequireAuthorization("Administrator")
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