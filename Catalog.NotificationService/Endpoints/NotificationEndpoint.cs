using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;

namespace Catalog.Web.Endpoints
{
    public class NotificationEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapNotificationEndpoints();
    }

    internal static class OrderEndpointDefinitionExtensions
    {
        public static async Task MapNotificationEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapGet("Test", async (
                CancellationToken cancellationToken) =>
                {
                    return Results.Ok("GetNotificationTestEndpoint");
                })
            .RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetNotificationTestEndpoint")
            .WithOpenApi();
        }
    }


}
