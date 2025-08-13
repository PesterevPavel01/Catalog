using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application;
using Catalog.Domain.Entities.Autorization;
using Catalog.Infrastructure;

namespace Catalog.Web.Endpoints
{
    public class ComponentEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapComponentEndpoints();
    }

    internal static class ComponentEndpointDefinitionExtensions
    {
        public static void MapComponentEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/Component/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(1, 0));

            group.MapGet("test",(
                HttpRequest request,
                ComponentServices propertyTypeServices,
                ApplicationDbContext applicationDbContext,
                CancellationToken cancellationToken) =>
            {
                return Results.Ok("Успешно");
            })
            .RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetTestEndpoint")
            .WithOpenApi();

            group.MapGet("all", async (
                HttpRequest request,
                ComponentServices propertyTypeServices,
                CancellationToken cancellationToken) =>
            {
                var result = await propertyTypeServices.GetAllAsync(cancellationToken);
                return Results.Ok(result);
            })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetAllComponentEndpoint")
            .WithOpenApi();

        }
    }


}
