using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Domain.Entities.Authorization;
using Catalog.ExchangeService.Application.Processors;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.ExchangeService.Endpoints.Customers
{
    public class UserQueryEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.UserQueryEndpoints();
    }

    internal static class UserQueryEndpointsDefinitionExtensions
    {
        public static async Task UserQueryEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/user/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0))
                .WithTags($"{nameof(ApplicationUser)} queries");

            group.MapGet("{userName}/external-id", async (
                [FromRoute] string userName,
                UserGetExternalIdProcessor processor,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(userName, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetExternalEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить код 1с"
            });

            group.MapGet("{userName}/roles", async (
                [FromRoute] string userName,
                UserRolesLoaderProcessor processor,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(userName, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("RolesEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить роли"
            });

            group.MapGet("unassigned", async (
                UnassignedUserLoaderProcessor processor,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("UnassignedUserEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Получить пользователей без роли"
            });
        }
    }
}