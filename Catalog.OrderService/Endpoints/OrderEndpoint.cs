using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events;
using Catalog.OrderService.Application.Configurations;
using Catalog.OrderService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Rebus.Bus;

namespace Catalog.OrderService.Endpoints
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
                [FromBody] CreateOrderDto model,
                IOptions <ApplicationConfiguration> applicationConfiguration,
                IBus bus,
                OrderCreatorProcessor orderProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await orderProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);
               
                await bus.Publish(new OrderCreatedEvent(result.Result.Code));
                //await bus.Publish(new OrderCreatedEvent(Guid.NewGuid().ToString()));

                return Results.Ok(result.Result);
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
                  ""orderItems"": [
                    {
                      ""moduleCode"": ""0aa3d8ce-3e61-4ca7-b10c-b3221f223b7b"",
                      ""quantity"": 3
                    }
                  ],
                  ""userName"": ""Administrator""
                }"
            });

            group.MapGet("GetAll", async (
                [FromServices] IBus bus,
                OrderLoaderProcessor orderLoaderProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await orderLoaderProcessor
                    .ProcessAsync(
                        predicate: x => x.Enabled,
                        cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                if (result.Ok)
                    await bus.Publish(new OrderCreatedEvent(result.Result.First().Code));

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetAllOrderEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Просмотр заказов."
            });
        }
    }


}
