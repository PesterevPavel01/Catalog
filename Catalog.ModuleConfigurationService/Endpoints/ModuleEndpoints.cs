using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Catalog.ModuleConfigurationService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace CCatalog.ModuleConfigurationService.Endpoints
{
    public class ModuleEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapModuleEndpoints();
    }

    internal static class ModuleComplectationEndpointDefinitionExtensions
    {
        public static async Task MapModuleEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/Module/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("Create", async (
                    [FromBody] CreateModuleDto model,
                    IUnitOfWork unitOfWork,
                    IBus bus,
                    ModuleCreatorProcessor creatorProcessor,
                    CancellationToken cancellationToken) =>
            {
                var result = await creatorProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                var module = await unitOfWork
                    .GetRepository<Module>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == result.Result.ModuleCode);

                await bus.Publish(new ApprovalCompletedEvent(Guid.NewGuid(), module.Id));

                return Results.Ok(result.Result);
            })
                //.RequireAuthorization("Administrator")
                .Produces(200)
                .ProducesProblem(401)
                .WithName("GetModuleCreateEndpoint")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Создание модуля.",
                    Description = @"{
                        ""moduleType"": ""Фасад"",
                        ""moduleTypeCode"": ""00000000FSD"",
                        ""numericParameters"": [
                        {
                            ""type"": ""Ширина"",
                            ""typeCode"": ""0000000WDHT"",
                            ""value"": 200
                        },
                        {
                            ""type"": ""Длина"",
                            ""typeCode"": ""000000LNGHT"",
                            ""value"": 400
                        }
                        ],
                        ""textParameters"": [
                        {
                            ""type"": ""Тон"",
                            ""typeCode"": ""000000000TN"",
                            ""value"": ""Матовый""
                        }
                        ]
                    }"
                });

            group.MapPost("AddComponent", async (
                [FromBody] ModuleComplectationDto model,
                ModuleComplectationProcessor moduleComplectationProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await moduleComplectationProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ModuleAddComponentEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Добавление компонента.",
                Description = @"
                {
                  ""moduleCode"": ""c2317912-df6c-4a21-8d1a-f451abf6ba29"",
                  ""componentCode"": ""00080185745""
                }<br><br>
                {
                  ""moduleCode"": ""c2317912-df6c-4a21-8d1a-f451abf6ba29"",
                  ""componentCode"": ""00080196471""
                }"
            });

            group.MapPost("GetAll", async (
                ModuleLoaderProcessor moduleLoaderProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await moduleLoaderProcessor
                    .ProcessAsync(
                        predicate: x => x.Enabled == true,
                        cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetAllModulesEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Просмотр модулей"
            });
        }
    }
}
