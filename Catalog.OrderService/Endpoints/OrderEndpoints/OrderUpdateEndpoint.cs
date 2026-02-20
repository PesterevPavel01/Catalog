using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Commands;
using Catalog.Contracts.Dto.Message;
using Catalog.Contracts.Enum;
using Catalog.Contracts.Events.OrderEvents;
using Catalog.Contracts.Resources;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Messages.OrderMessages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.OrderService.Endpoints.OrderEndpoints
{
    public class OrderUpdateEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderUpdateEndpoints();
    }

    internal static class OrderUpdateEndpointDefinitionExtensions
    {
        public static async Task MapOrderUpdateEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0))
                .WithTags($"{nameof(Order)} commands");

            group.MapPatch("add-message",
                    async (
                        [FromBody] CreateMessageDto model,
                        IMediator mediator,
                        IBus bus,
                        HttpContext context) =>
                    {
                        var result = await mediator.Send(new AddMessage.Request(model), context.RequestAborted);

                        if (!result.Ok)
                            return Results.BadRequest(result.Error);

                        await bus.Publish(new OrderAddMessageEvent(result.Result));

                        return Results.Ok(result.Result);
                    })
                //.RequireAuthorization("Constructor")
                .Produces(200)
                .ProducesProblem(401)
                .WithName("OrderAddMessageEndpoint")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Добавить комментарий к элементу заказа."
                });

            group.MapPatch("produced",
                    async (
                        [FromBody] IEnumerable<string> codes,
                        IBus bus) =>
                    {
                        foreach(var code in codes)
                            await bus.Publish(new CreateOrderEventCommand(code, OrderEventType.Produced, OrderEventTypeTitles.Produced));

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
