using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.Application;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.Domain.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Web.Endpoints
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
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/Autorize/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("Autenticate", async (
            [FromBody] LoginDto model,
                [FromServices] AuthentificationProcessor authentificationProcessor,
                ComponentServices propertyTypeServices,
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

            //group.MapPost("RefreshToken", async (
            //    [FromBody] TokenDto model,
            //    [FromServices] IUnitOfWork unitOfWork,
            //    ComponentServices propertyTypeServices,
            //    CancellationToken cancellationToken) =>
            //{
            //    var result = "Метод для RefreshToken";
            //    return Results.Ok(result);
            //})
            //.Produces(200)
            //.ProducesProblem(401)
            //.WithName("RefreshTokenEndpoint")
            //.WithOpenApi(operation => new(operation)
            //{
            //    Summary = "Обновление токена",
            //});
        }
    }
}
