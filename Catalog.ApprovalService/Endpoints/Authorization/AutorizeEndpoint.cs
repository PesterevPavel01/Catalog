using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.Domain.Dto.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.ApprovalService.Endpoints.Authorization
{
    public class AutorizeEndpoint : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapAutorizeEndpoints();
    }

    public static class AutorizeEndpointDefinitionExtensions
    {
        public static void MapAutorizeEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/autorize/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(1, 0));

            group.MapPost("autenticate", async (
            [FromBody] LoginDto model,
                [FromServices] AuthentificationProcessor authentificationProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await authentificationProcessor.ProcessAsync(model, cancellationToken);
                return Results.Ok(result);
            })
            .Produces(200)
            .ProducesProblem(401)
            .WithName("AutenticateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Аутентификация пользователя",
                Description = @"Пример запроса:<br>
                    {
                    ""userName"": ""Administrator"",
                    ""password"": ""Qwerty1234!""
                    }"
            });
        }
    }
}
