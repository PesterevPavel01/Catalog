using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Events;
using Catalog.Domain.Entities;
using Catalog.ModuleConfigurationService.Application.Description;
using Catalog.ModuleConfigurationService.Application.Managers;
using Catalog.ModuleConfigurationService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.ModuleConfigurationService.Endpoints
{
    public class ModuleCreationEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapModuleCreateEndpoints();
    }

    internal static class ModuleCreateEndpointDefinitionExtensions
    {
        public static async Task MapModuleCreateEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/modules/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("create", async (
                    [FromBody] CreateModuleDto model,
                    IBus bus,
                    IUnitOfWork unitOfWork,
                    ModuleCreateManager createManager,
                    CancellationToken cancellationToken) =>
            {
                var result = await createManager.CreateAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                var module = await unitOfWork
                    .GetRepository<Module>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == result.Result.ModuleCode);

                await bus.Publish(new ModuleCreatedEvent(module.Id));

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ModuleCreateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создание модуля.",
                Description = ComponentCreateDescription.GetValue()
            });
        }
    }
}
