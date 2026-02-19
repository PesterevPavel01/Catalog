using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto;
using Catalog.Contracts.Dto.Events;
using Catalog.Contracts.Entities;
using Catalog.OrderService.Application.Commands;
using Catalog.OrderService.Application.Messages.OrderEventMessages;
using Catalog.Redis;
using MediatR;
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
                .HasApiVersion(new ApiVersion(2, 0))
                .WithTags($"{nameof(OrderEvent)} queries"); ;

            group.MapGet("constructor/{pageIndex}/{pageSize}",
                async (
                    [FromRoute] Int16 pageIndex,
                    [FromRoute] Int16 pageSize,
                    IBus bus,
                    IMediator mediator,
                    HttpContext context,
                    RedisServiceFactory redisServiceFactory,
                    CancellationToken cancellationToken) =>
                {
                    string? cacheKey = null;

                    if (pageIndex == 0 && pageSize == OrderEvent.CacheEntriesCount)
                    {
                        var redisService = redisServiceFactory.GetService<OrderEventDto>();

                        cacheKey = OrderEvent.GenerateConstructorCommonCacheKey(redisService.GenerateCacheKey);

                        var cachedEvents = await redisService.GetFromCacheAsync(cacheKey, cancellationToken);

                        if (cachedEvents.Ok)
                            return Results.Ok(new PagedResponseDto<OrderEventDto>(
                                cachedEvents.Result,
                                cachedEvents.Result.Count(),
                                pageIndex,
                                pageSize));
                    }

                    var result = await mediator.Send(new GetOrderEvents.Request(pageIndex, pageSize), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    if (cacheKey is not null)
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
                    IMediator mediator,
                    HttpContext context,
                    CancellationToken cancellationToken) =>
                {
                    string? cacheKey = null;

                    if (pageIndex == 0 && pageSize == OrderEvent.CacheEntriesCount)
                    {
                        var redisService = redisServiceFactory.GetService<OrderEventDto>();

                        cacheKey = OrderEvent.GenerateUserCommonCacheKey(redisService.GenerateCacheKey, userName);

                        var cachedEvents = await redisService.GetFromCacheAsync(cacheKey, cancellationToken);

                        if (cachedEvents.Ok)
                            return Results.Ok(new PagedResponseDto<OrderEventDto>(
                                cachedEvents.Result,
                                cachedEvents.Result.Count(),
                                pageIndex,
                                pageSize));
                    }

                    var result = await mediator.Send(new GetUserOrderEvents.Request(userName, pageIndex, pageSize), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    if(cacheKey is not null)
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
