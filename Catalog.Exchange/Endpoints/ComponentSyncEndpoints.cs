using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Components;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Messaging.ComponentMessages.Queries;
using Catalog.ExchangeService.Application.Messaging.ComponentSyncMessages.Queries;
using Catalog.ExchangeService.Application.Messaging.OrderSyncMessages;
using Catalog.ExchangeService.Application.Processors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.ExchangeService.Endpoints
{
    public class ComponentSyncEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapComponentSyncEndpoints();
    }

    internal static class ComponentDefinitionExtensions
    {
        public static async Task MapComponentSyncEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(3, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/components/sync/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(3, 0))
                .WithTags(nameof(Component));

            group.MapPost("", async ([FromBody] IEnumerable<ComponentDto> models, IMediator mediator,HttpContext context)
                     => 
                    {
                        var result = await mediator.Send(new PostComponentSyncSession.Request(models), context.RequestAborted);

                        if (!result.Ok)
                            return Results.BadRequest(result.Error);

                        return Results.Ok(result.Result);
                    })
                .RequireAuthorization("Administrator")
                .Produces(200)
                .ProducesProblem(401)
                .WithName("SetComponentEndpoint")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Добавить или обновить компонент."
                });

            group.MapGet("session/{code}", async ( IMediator mediator, string code, HttpContext context) 
                    =>
                    {
                        var result = await mediator.Send(new GetComponentSyncSessionByCode.Request(code), context.RequestAborted);

                        if (!result.Ok)
                            return Results.BadRequest(result.Error);

                        return Results.Ok(result.Result);
                    })
                .RequireAuthorization("Administrator")
                .Produces(200)
                .ProducesProblem(401)
                .WithName("QueryComponentSyncResultEndpoint")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Получить результат обмена."
                });
        }
    }
}
