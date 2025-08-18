using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors;
using Catalog.Domain.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Web.Endpoints
{
    public class ModuleEndpoint: AppDefinition
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

            var group = routes.MapGroup("/api/v{version:apiVersion}/Module/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapGet("Create", async (
                ModuleCreatorProcessor moduleProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await moduleProcessor.ProcessAsync(cancellationToken);
                return Results.Ok("Успешно");
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetModuleCreateEndpoint")
            .WithOpenApi();

            group.MapGet("Test", async (
                CancellationToken cancellationToken) =>
                {
                    return Results.Ok("GetModuleTestEndpoint");
                })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetModuleTestEndpoint")
            .WithOpenApi();
        }
    }


}
