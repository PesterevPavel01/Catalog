using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors;
using Catalog.Contracts.Events;
using Catalog.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.Web.Endpoints
{
    public class OrderEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderEndpoints();
    }

    internal static class OrderEndpointDefinitionExtensions
    {
        public static async Task MapOrderEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/Order/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("Create", async (
                [FromBody] OrderDto model,
                [FromServices] IBus bus,
                OrderCreatorProcessor orderProcessor,
                CancellationToken cancellationToken) =>
            {
                /*var result = await orderProcessor.ProcessAsync(model, cancellationToken);

                if(result.Ok)
                    await bus.Publish(new OrderCreatedEvent(result.Result.OrderCode));*/

                await bus.Publish(new OrderCreatedEvent(Guid.NewGuid().ToString()));

                return Results.Ok();
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetCreateEndpoint")
            .WithOpenApi();

            group.MapGet("Test", async (
                [FromServices] IBus bus,
                CancellationToken cancellationToken) =>
            {
                await bus.Publish(new OrderCreatedEvent(Guid.NewGuid().ToString()));

                return Results.Ok("TestController");
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetTestEndpoint")
            .WithOpenApi();
        }
    }


}
