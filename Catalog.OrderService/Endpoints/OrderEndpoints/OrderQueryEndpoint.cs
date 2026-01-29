using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.OrderService.Application.Managers;
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
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapGet("by-period/{days:int:min(1):max(365)}",
                async (
                    [FromRoute] int days,
                    OrderQueriesManager orderQueriesManager,
                    CancellationToken cancellationToken,
                    [FromQuery] string? titlePattern = null,
                    [FromQuery] bool ascending = false,
                    [FromQuery] bool incompleteOnly = false,
                    [FromQuery] bool customOnly = false,
                    [FromQuery] string[] ? customers = null,
                    [FromQuery] string[]? statuses = null,
                    [FromQuery] int pageSize = 20,
                    [FromQuery] int pageIndex = 0) =>
                {
                    var result = await orderQueriesManager.HandleAsync(
                         days: days,
                         customers: customers,
                         statuses: statuses,
                         cacheKeyType: "constructor",
                         titlePattern: titlePattern,
                         ascending: ascending,
                         incompleteOnly: incompleteOnly,
                         customOnly: customOnly,
                         pageSize: pageSize,
                         pageIndex: pageIndex,
                         cancellationToken: cancellationToken);

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
                    OrdersQueryHandler queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var ordersResult = await queryHandler
                        .HandleAsync(
                            code: orderCode,
                            cancellationToken: cancellationToken);

                    if (!ordersResult.Ok)
                        return Results.BadRequest(ordersResult.Error);

                    return Results.Ok(ordersResult.Result.Select(x => x.ConvertToDto()));
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
                    OrderQueriesManager orderQueriesManager,
                    CancellationToken cancellationToken,
                    [FromQuery] string? titlePattern = null,
                    [FromQuery] bool ascending = false,
                    [FromQuery] bool incompleteOnly = false,
                    [FromQuery] bool customOnly = false,
                    [FromQuery] int pageSize = 20,
                    [FromQuery] int pageIndex = 0) =>
                {
                    var result = await orderQueriesManager.HandleAsync(
                        days: days, 
                        cacheKeyType: userLogin,
                        titlePattern: titlePattern,
                        userLogin: userLogin,
                        ascending: ascending,
                        incompleteOnly: incompleteOnly,
                        customOnly: customOnly,
                        pageSize: pageSize,
                        pageIndex: pageIndex,
                        cancellationToken: cancellationToken);

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
                    OrderMessagesQueryHandler queryHandler,
                    CancellationToken cancellationToken) =>
                {
                    var result = await queryHandler.HandleAsync(orderCode, cancellationToken);

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
