using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.OrderService.Application.Messages.OrderMessages;
using Catalog.Redis;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.OrderService.Endpoints.OrderEndpoints
{
    public class OrderQueryEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapOrderQueryEndpoints();
    }

    internal static class OrderQueryEndpointDefinitionExtensions
    {
        public static async Task MapOrderQueryEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0))
                .WithTags($"{nameof(Order)} queries");

            group.MapGet("by-period/{days:int:min(1):max(365)}",
                async (
                    [FromRoute] int days,
                    IMediator mediator,
                    HttpContext context,
                    RedisServiceFactory redisServiceFactory,
                    [FromQuery] string? titlePattern = null,
                    [FromQuery] bool ascending = false,
                    [FromQuery] bool incompleteOnly = false,
                    [FromQuery] bool customOnly = false,
                    [FromQuery] string[] ? customers = null,
                    [FromQuery] short[]? statuses = null,
                    [FromQuery] int pageSize = 20,
                    [FromQuery] int pageIndex = 0) =>
                {
                    var request = new GetOrders.Request(
                         Days: days,
                         Customers: customers,
                         Statuses: statuses,
                         CacheKey: Order.GenerateConstructorCommonCacheKey(redisServiceFactory
                            .GetService<OrderDto>()
                            .GenerateCacheKey),
                         TitlePattern: titlePattern,
                         Ascending: ascending,
                         IncompleteOnly: incompleteOnly,
                         CustomOnly: customOnly,
                         PageSize: pageSize,
                         PageIndex: pageIndex);

                    var result = await mediator.Send(request, context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetAllOrderEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Просмотр заказов за период."
            });

            group.MapGet("by-code/{orderCode}", 
                async (
                    [FromRoute] string orderCode,
                    OrderQueryHandler queryHandler,
                    CancellationToken cancellationToken,
                    [FromQuery] bool cache = false) =>
                {
                    var ordersResult = await queryHandler
                        .HandleAsync(
                            code: orderCode,
                            cache: cache,
                            cancellationToken: cancellationToken);

                    if (!ordersResult.Ok)
                        return Results.BadRequest(ordersResult.Error);

                    return Results.Ok(new List<OrderDto>() { ordersResult.Result });
                })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetByCodeOrderEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить заказ по коду."
            });

            group.MapGet("by-customer/{userLogin}/{days:int:min(1):max(365)}",
                async (
                    [FromRoute] string userLogin,
                    [FromRoute] int days,
                    IMediator mediator,
                    HttpContext context,
                    RedisServiceFactory redisServiceFactory,
                    [FromQuery] string? titlePattern = null,
                    [FromQuery] bool ascending = false,
                    [FromQuery] bool incompleteOnly = false,
                    [FromQuery] bool customOnly = false,
                    [FromQuery] int pageSize = 20,
                    [FromQuery] int pageIndex = 0) =>
                {
                    var request = new GetOrders.Request(
                        Days: days,
                        CacheKey: Order.GenerateUserCommonCacheKey(redisServiceFactory
                            .GetService<OrderDto>()
                            .GenerateCacheKey, userLogin),
                        TitlePattern: titlePattern,
                        UserLogin: userLogin,
                        Ascending: ascending,
                        IncompleteOnly: incompleteOnly,
                        CustomOnly: customOnly,
                        PageSize: pageSize,
                        PageIndex: pageIndex);

                    var result = await mediator.Send(request, context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrdersByCustomerLoginEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить заказы пользователя за период"
            });

            group.MapGet("{orderCode}/messages",
                async (
                    [FromRoute] string orderCode,
                    IMediator mediator,
                    HttpContext context) =>
                {
                    var result = await mediator.Send(new GetMessages.Request(orderCode), context.RequestAborted);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrdersMessagesQueryEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить комментарии заказа."
            });
        }
    }
}
