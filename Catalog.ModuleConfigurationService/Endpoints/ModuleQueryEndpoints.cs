using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.ModuleConfigurationService.Application.Processors;

namespace Catalog.ModuleConfigurationService.Endpoints
{
    public class ModuleQueryEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapModuleQueryEndpoints();
    }

    internal static class ModuleQueryEndpointsDefinitionExtensions
    {
        public static async Task MapModuleQueryEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/module/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("all", async (
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