using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto.Components;
using Catalog.ExchangeService.Application.Description;
using Catalog.ExchangeService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
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
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/component/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

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
                Description = ComponentCreateEndpointDescription.Description
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

            group.MapGet("all", async (
                ComponentLoaderProcessor componentLoaderProcessor,
                CancellationToken cancellationToken) =>
                {
                    var result = await componentLoaderProcessor
                        .ProcessAsync(
                            cancellationToken: cancellationToken, 
                            predicate: x => x.Enabled == true);

                    if (!result.Ok)
                        return Results.BadRequest(result.Error);

                    return Results.Ok(result.Result);
                })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetAllModuleEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Просмотр компонентов"
            });
        }
    }


}
