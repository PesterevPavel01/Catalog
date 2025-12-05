using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.ComponentService.Application.Command;
using Catalog.ComponentService.Application.Processors;
using Catalog.Contracts.Dto.Components;
using Catalog.Domain.Entities;
using Catalog.ExchangeService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Rebus.Bus;

namespace Catalog.Web.Endpoints
{
    public class ComponentEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapComponentEndpoints();
    }
    internal static class ComponentEndpointDefinitionExtensions
    {
        public static async Task MapComponentEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(3, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/components/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(3, 0));

            group.MapPost("create", async (
                [FromBody] ComponentDto model,
                IBus bus,
                ComponentCreatorProcessor componentCreatorProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await componentCreatorProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ComponentCreateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создание компонента",
                //Description = ComponentCreateEndpointDescription.Description
            });

            group.MapPost("add-numeric-parameter", async (
                [FromBody] ComponentAddNumericParameterDto model,
                IBus bus,
                ComponentAddNumericParameterProcessor componentAddNumericParameterProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await componentAddNumericParameterProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("AddNumericParameterEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Добавить числовой параметр",
                Description = @"{
                  ""componentCode"": ""00080185745"",
                  ""numericParameters"": [
                    {
                      ""type"": ""Максимальная ширина"",
                      ""typeCode"": ""0000MXWDHT"",
                      ""value"": 1200
                    }
                  ]
                }"
            });

            group.MapPost("add-text-parameter", async (
                [FromBody] ComponentAddTextParameterDto model,
                ComponentAddTextParameterProcessor componentAddTextParameterProcessor,
                IBus bus,
                CancellationToken cancellationToken) =>
            {
                var result = await componentAddTextParameterProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("AddTextParameterEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Добавить текстовый параметр",
                Description = @"{
                  ""componentCode"": ""00080185745"",
                  ""textParameters"": [
                    {
                      ""type"": ""CUSTOM COMPONENT"",
                      ""typeCode"": ""0000000CSTM"",
                      ""value"": ""CUSTOM COMPONENT""
                    }
                  ]
                }"
            });

            group.MapGet("all", 
                async (
                    IBus bus,
                    CachedComponentLoaderProcessor cachedComponentLoaderProcessor,
                    ComponentLoaderProcessor componentLoaderProcessor,
                    CancellationToken cancellationToken,
                    [FromQuery] bool ascending = false) =>
                {
                    var cacheKey = cachedComponentLoaderProcessor.GenerateCacheKey(("entity", "Component"), ("type", "all"), ("ascending", ascending));

                    var cachedComponents = await cachedComponentLoaderProcessor.GetComponentsAsync(cacheKey, cancellationToken);

                    if (cachedComponents.Ok)
                        return Results.Ok(cachedComponents.Result);

                    var result = await componentLoaderProcessor
                        .ProcessAsync(
                            cancellationToken: cancellationToken, 
                            predicate: x => x.Enabled == true);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Send(new SetComponentsInCacheCommand(cacheKey, result.Result));

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetAllModuleEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Просмотр компонентов."
            });

            group.MapGet("by-type/{typeCode}",
                async (
                    [FromRoute] string typeCode,
                    IBus bus,
                    CachedComponentLoaderProcessor cachedComponentLoaderProcessor,
                    ComponentLoaderProcessor componentLoaderProcessor,
                    CancellationToken cancellationToken,
                    [FromQuery] bool ascending = false) =>
                {
                    var cacheKey = cachedComponentLoaderProcessor.GenerateCacheKey(("entity", "Component"), ("type", typeCode), ("ascending", ascending));

                    var cachedComponents = await cachedComponentLoaderProcessor.GetComponentsAsync(cacheKey, cancellationToken);

                    if (cachedComponents.Ok)
                        return Results.Ok(cachedComponents.Result);

                    var result = await componentLoaderProcessor
                        .ProcessAsync(
                            cancellationToken: cancellationToken,
                            ascending: ascending,
                            predicate: x => x.Enabled == true && x.ComponentType.Code == typeCode
                            && x.ComponentTextParameters.FirstOrDefault(tp => tp.ParameterType.Code == Component.CustomParameterTypeCode) == null);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    await bus.Send(new SetComponentsInCacheCommand(cacheKey, result.Result));

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ComponentByTypeEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить компоненты по типу."
            });

            group.MapDelete("/remove/{componentCode}",
                async (
                    [FromRoute] string componentCode,
                    ComponentRemovalProcessor processor,
                    CancellationToken cancellationToken) =>
                {
                    var result = await processor.ProcessAsync(componentCode, cancellationToken);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("OrderItemRemoveEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Удалить компонент."
            });

            group.MapPatch("text-parameter-replace", async (
                [FromBody] ComponentDto model,
                IUnitOfWork unitOfWork,
                TextParametersReplacer updateProcessor,
                CancellationToken cancellationToken) =>
                {
                    var operationResult = await updateProcessor.ProcessAsync(model, cancellationToken);

                    //await bus.Publish(new ModuleChangedEvent(module.Id));

                    if(!operationResult.Ok)
                        return Results.BadRequest(operationResult.Error);

                    return Results.Ok(operationResult.Result);
                })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ComponentTextParameterReplaceEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Замена текстовых параметров.",
            });

        }
    }
}
