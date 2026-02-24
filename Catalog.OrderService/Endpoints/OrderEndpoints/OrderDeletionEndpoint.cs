using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Messages.OrderMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Config;

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
                .HasApiVersion(new ApiVersion(2, 0))
                .WithTags($"{nameof(Order)} commands");

            group.MapPatch("{orderCode}/disable", 
                async (
                    [FromRoute] string orderCode,
                    IMediator mediator,
                    IBus bus,
                    HttpContext context) =>
                {
                    var result = await mediator.Send(new DisableOrder.Request(orderCode), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Publish(new OrderDisabledEvent(result.Result));

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

            group.MapGet("cleanup-old-orders",
                async (
                    HttpContext context,
                    IOptions <OrderConfiguration> options,
                    IBus bus,
                    IMediator mediator) =>
                {
                    var archiveStorageDays = options.Value.OrderCleanupSettings;

                    var result = await mediator.Send(new CleanupOldOrder.Request(archiveStorageDays), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Publish(new CleanupOldOrderEvent());

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("CleanupOldOrderEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Проверка старых заказов"
            });
        }
    }
}
