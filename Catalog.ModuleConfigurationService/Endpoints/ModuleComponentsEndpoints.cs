using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Catalog.ModuleConfigurationService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.ModuleConfigurationService.Endpoints
{
    public sealed class ModuleComponentsEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapModuleComponentsEndpoints();
    }

    internal static class ModuleComponentsEndpointsDefinitionExtensions
    {
        public static async Task MapModuleComponentsEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/module/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("add-component", async (
                [FromBody] ModuleComplectationDto model,
                IBus bus,
                IUnitOfWork unitOfWork,
                ModuleComplectationProcessor moduleComplectationProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await moduleComplectationProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                var module = await unitOfWork
                    .GetRepository<Module>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == result.Result.ModuleCode);

                await bus.Publish(new ModuleUpdatedEvent(module.Id));

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
                }"
            });

            group.MapDelete("remove-component", async (
                [FromBody] ModuleComplectationDto model,
                IBus bus,
                IUnitOfWork unitOfWork,
                ModuleRemoveComponentProcessor removeComponentProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await removeComponentProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                var module = await unitOfWork
                    .GetRepository<Module>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == result.Result.ModuleCode);

                await bus.Publish(new ModuleUpdatedEvent(module.Id));

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ModuleRemoveComponentEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Удаление компонента.",
                Description = @"
                {
                  ""moduleCode"": ""c2317912-df6c-4a21-8d1a-f451abf6ba29"",
                  ""componentCode"": ""00080185745""
                }"
            });
        }
    }
}