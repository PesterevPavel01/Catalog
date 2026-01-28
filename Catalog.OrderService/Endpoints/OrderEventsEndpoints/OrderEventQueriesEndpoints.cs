using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto;
using Catalog.Contracts.Dto.Events;
using Catalog.OrderService.Application.Commands;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.Redis;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.OrderService.Endpoints.EventsEndpoints
{       
    public class OrderEventQueriesEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderEventQueriesEndpoints();
    }

    internal static class OrderEventQueriesEndpointsDefinitionExtensions
    {
        public static async Task MapOrderEventQueriesEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/events/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapGet("constructor/{pageIndex}/{pageSize}",
                async (
                    [FromRoute] Int16 pageIndex,
                    [FromRoute] Int16 pageSize,
                    IBus bus,
                    RedisServiceFactory redisServiceFactory,
                    ConstructorOrderEventQueriesHandler commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    var redisService = redisServiceFactory.GetService<OrderEventDto>();

                    var cacheKey = redisService.GenerateCacheKey(("type", "constructor"));

                    var cachedEvents = await redisService.GetFromCacheAsync(cacheKey, cancellationToken);

                    if (cachedEvents.Ok)
                        return Results.Ok(new PagedResponseDto<OrderEventDto>(
                            cachedEvents.Result
                                .Skip(pageIndex * pageSize)
                                .Take(pageSize),
                            cachedEvents.Result.Count(),
                            pageIndex,
                            pageSize));

                    var result = await commandHandler.HandleAsync(cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Send(new CacheOrderEventsCommand(cacheKey, result.Result.Items));

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ConstructorOrderEventEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Посмотреть последние события по заказам конструктора."
            });

            group.MapGet("{userName}/{pageIndex}/{pageSize}",
                async (
                    [FromRoute] string userName,
                    [FromRoute] Int16 pageIndex,
                    [FromRoute] Int16 pageSize,
                    IBus bus,
                    RedisServiceFactory redisServiceFactory,
                    ApplicationUserOrderEventQueriesHandler commandHandler,
                    CancellationToken cancellationToken) =>
                {
                    var redisService = redisServiceFactory.GetService<OrderEventDto>();

                    var cacheKey = redisService.GenerateCacheKey(("type", userName));

                    var cachedEvents = await redisService.GetFromCacheAsync(cacheKey, cancellationToken);

                    if (cachedEvents.Ok)
                        return Results.Ok(new PagedResponseDto<OrderEventDto>(
                            cachedEvents.Result
                                .Skip(pageIndex * pageSize)
                                .Take(pageSize),
                            cachedEvents.Result.Count(),
                            pageIndex,
                            pageSize));

                    var result = await commandHandler.HandleAsync(userName, cancellationToken, pageIndex, pageSize);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Send(new CacheOrderEventsCommand(cacheKey, result.Result.Items));

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("CustomerOrderEventEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Посмотреть последние события по заказам клиента."
            });
        }
    }
}
