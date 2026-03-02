using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Commands.Exchange;
using Catalog.Contracts.Dto.Exchange;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Messaging.OrderSyncMessages;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;
using System;

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
                        IMediator mediator,
                        HttpContext context) =>
                    {
                        var orders = await mediator.Send( new GetLastModifiedOrders.Request("Обмен заказами с 1с"), context.RequestAborted);

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
                        IMediator mediator,
                        HttpContext context) =>
                    {
                        var result = await mediator.Send(new ConfirmOrderSync.Request(syncResult), context.RequestAborted);

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

            group.MapPatch("produced",
                async (
                    [FromBody] IEnumerable<string> codes,
                    IBus bus,
                    CancellationToken cancellationToken) =>
                {
                    await bus.Publish(new MarkOrdersAsProducedCommand(codes));

                    return Results.Ok();
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ProducedOrdersEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Установить заказам статус \"Производство завершено\"."
            });
        }
    }


}