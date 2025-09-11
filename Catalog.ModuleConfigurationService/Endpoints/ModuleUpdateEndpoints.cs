using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Catalog.ModuleConfigurationService.Application.Description;
using Catalog.ModuleConfigurationService.Application.Managers;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.ModuleConfigurationService.Endpoints
{
    public class ModuleUpdateEndpoints: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapModuleUpdateEndpoints();
    }

    internal static class ModuleUpdateEndpointDefinitionExtensions
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
                ModuleUpdateManager updateManager,
                CancellationToken cancellationToken) =>
            {
                var operationResult = await updateManager.UpdateAsync(model, cancellationToken);

                var module = await unitOfWork
                    .GetRepository<Module>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == model.ModuleCode,
                        include: Module.IncludeRequiredField()
                    );

                await bus.Publish(new ModuleUpdatedEvent(module.Id));

                return Results.Ok(operationResult.Result);
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
