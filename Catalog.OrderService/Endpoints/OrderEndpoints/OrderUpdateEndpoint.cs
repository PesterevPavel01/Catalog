using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Message;
using Catalog.OrderService.Application.Handlers.CommandHandlers;
using Microsoft.AspNetCore.Mvc;

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
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPatch("add-message",
                async (
                    [FromBody] CreateMessageDto model,
                    AddMessageCommandHandler commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await commandHandler.ExecuteAsync(model, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

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
        }
    }


}
