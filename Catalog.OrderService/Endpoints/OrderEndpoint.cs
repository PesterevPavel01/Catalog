using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events;
using Catalog.OrderService.Application.Configurations;
using Catalog.OrderService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
                [FromServices] IOptions <ApplicationConfiguration> applicationConfiguration,
                [FromBody] OrderDto model,
                [FromServices] IBus bus,
                OrderCreatorProcessor orderProcessor,
                CancellationToken cancellationToken) =>
            {
                /*var result = await orderProcessor.ProcessAsync(model, cancellationToken);

                if(result.Ok)
                    await bus.Publish(new OrderCreatedEvent(result.Result.OrderCode));*/

                //await bus.Publish(new OrderCreatedEvent(Guid.NewGuid().ToString()));

                return Results.Ok(applicationConfiguration.Value);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetCreateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создание нового заказа.",
                Description = @"
                {
                  ""moduleCode"": ""35a56634-cc84-4aef-94a2-5e9da07a16d0"",
                  ""componentCode"": ""00080196471""
                }"
            });

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
