using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.OrderService.Application.Handlers.QueryHandlers;
using Catalog.OrderService.Application.Processors;
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
                    OrderLoaderProcessor orderLoaderProcessor,
                    CancellationToken cancellationToken,
                    [FromQuery] bool ascending = false,
                    [FromQuery] bool incompleteOnly = false,
                    [FromQuery] bool customOnly = false) =>
                {
                    var ordersResult = await orderLoaderProcessor
                        .ProcessAsync(
                            predicate: x => x.Enabled && x.CreatedAt > DateTime.Now.AddDays(-1* days),
                            ascending, incompleteOnly, customOnly,cancellationToken);

                    if (!ordersResult.Ok)
                        return Results.BadRequest(ordersResult.Error);

                    var result = ordersResult.Result.Select(x => new CommonOrderDto()
                    {
                        Code = x.Code,
                        Title = x.Title.Value,
                        UserName = x.ApplicationUser.UserName,
                        IsCompleted = x.IsCompleted(),
                        IsCustom = x.OrderItems.FirstOrDefault(item => item.Module.IsCustom == true) is not null,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    });

                    return Results.Ok(result);
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
                    OrderLoaderProcessor orderLoaderProcessor,
                    CancellationToken cancellationToken) =>
                {
                    var ordersResult = await orderLoaderProcessor
                        .ProcessAsync(
                            predicate: x => x.Enabled
                            && x.Code == orderCode,
                            cancellationToken: cancellationToken);

                    if (!ordersResult.Ok)
                        return Results.BadRequest(ordersResult.Error);

                    var result = ordersResult.Result.Select(x => x.ConvertToDto());

                    return Results.Ok(result);
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
                    OrdersByCustomerLoginProcessor processor,
                    CancellationToken cancellationToken, 
                    [FromQuery] bool ascending = false,
                    [FromQuery] bool incompleteOnly = false,
                    [FromQuery] bool customOnly = false) =>
                {
                    var result = await processor.ProcessAsync(userLogin, days, ascending, incompleteOnly, customOnly,  cancellationToken);

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
