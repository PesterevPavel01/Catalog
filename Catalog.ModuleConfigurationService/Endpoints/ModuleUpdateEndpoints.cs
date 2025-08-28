using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Catalog.ModuleConfigurationService.Application.Description;
using Catalog.ModuleConfigurationService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.ModuleConfigurationService.Endpoints
{
    public class ModuleUpdateEndpoints: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapModuleUpdateEndpoints();
    }

    internal static class ModuleComplectationEndpointDefinitionExtensions
    {
        public static async Task MapModuleUpdateEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/module/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPatch("update", async (
                [FromBody] UpdateModuleDto model,
                IBus bus,
                IUnitOfWork unitOfWork,
                ModuleDataPurgeProcessor dataPurgeProcessor,
                ModuleUpdaterProcessor moduleUpdaterProcessor,
                ModuleComplectationProcessor complectationProcessor,
                CancellationToken cancellationToken) =>
            {
                var operationResult = await dataPurgeProcessor.ProcessAsync(model.ModuleCode);

                if (!operationResult.Ok)
                    return Results.BadRequest(operationResult.Error);

                operationResult = await moduleUpdaterProcessor.ProcessAsync(model, cancellationToken);

                if (!operationResult.Ok)
                    return Results.BadRequest(operationResult.Error);

                foreach (var component in model.Components)
                {
                    operationResult = await complectationProcessor
                    .ProcessAsync(
                        new()
                        {
                            ComponentCode = component.ComponentCode,
                            ModuleCode = model.ModuleCode,
                        },
                    cancellationToken);

                    if (!operationResult.Ok)
                        return Results.BadRequest(operationResult.Error);
                }

                var module = await unitOfWork
                    .GetRepository<Module>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == model.ModuleCode,
                        include: Module.IncludeRequaredField()
                    );

                await bus.Publish(new ModuleUpdatedEvent(module.Id));

                return Results.Ok(module.ConvertToDto());
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ModuleUpdateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Обновление модуля.",
                Description = ComponentCreateDescription.GetValue()
            });
        }
    }
}
