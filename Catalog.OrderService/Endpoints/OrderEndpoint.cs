using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Events.OrderEvents;
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

            var group = routes.MapGroup("/api/v{version:apiVersion}/orders/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("create",
                async (
                    [FromBody] CreateOrderDto model,
                    IOptions <ApplicationConfiguration> applicationConfiguration,
                    OrderCreatorProcessor orderProcessor,
                    OrderItemCreatorProcessor orderItemProcessor,
                    CancellationToken cancellationToken) =>
                {
                    var result = await orderProcessor.ProcessAsync(model, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    if (model.OrderItems.Any())
                    {
                        var operationResult = await orderItemProcessor.ProcessAsync(model.OrderItems, cancellationToken);

                        if (!operationResult.Ok)
                            return Results.BadRequest(operationResult.Error);
                    }

                    return Results.Ok(result.Result);
                })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderCreateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создание нового заказа.",
            });

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
                        UpdatedAt = x.UpdatedAt,
                        Messages = x.OrderItems.SelectMany(x => x.Messages.Select(message => message.ConvertToDto()))
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

            group.MapPatch("{orderCode}/disable", 
                async (
                    [FromRoute] string orderCode,
                    OrderDisableProcessor processor,
                    CancellationToken cancellationToken) =>
                {
                    var result = await processor.ProcessAsync(orderCode, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderDisableEndpoint")
            .WithOpenApi(operation => new(operation)
            {
            Summary = "Деактивировать заказ"
            });
        }
    }


}
