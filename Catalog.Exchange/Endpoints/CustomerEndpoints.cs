using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Application.Processors.AuthorizationProcessor;
using Catalog.Contracts.Dto.Authorization;
using Catalog.Contracts.Events.CustomerEvents;
using Catalog.ExchangeService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.ExchangeService.Endpoints
{
    public class UserEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapCustomerEndpoints();
    }
    internal static class UserEndpointDefinitionExtensions
    {
        public static async Task MapCustomerEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/user/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("set-customer-role", async (
                [FromBody] UserDto model,
                UserSetRoleProcessor processor,
                CancellationToken cancellationToken) =>
            {
                model.Roles = ["customer"];
                var result = await processor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            .RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("SetCustomerRoleEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Присвоить роль customer"
            });

            group.MapPost("set-constructor-role", async (
                [FromBody] UserDto model,
                UserSetRoleProcessor processor,
                CancellationToken cancellationToken) =>
            {
                model.Roles = ["constructor"];

                var result = await processor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            .RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("SetConstructorRoleEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Присвоить роль constructor"
            });

            group.MapPost("create", async (
                [FromBody] RegistrationUserDto model,
                RegistrationProcessor processor,
                IBus bus,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                await bus.Publish(new CustomerCreatedEvent(model));

                return Results.Ok();
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("CreateUserEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создать пользователя"
            });

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

            group.MapPatch("{userName}/disable", async (
                [FromRoute] string userName,
                UserDisableProcessor processor,
                CancellationToken cancellationToken) =>
            {
                var result = await processor.ProcessAsync(userName, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
            .RequireAuthorization("Constructor")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("UserDeleteEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Деактивировать пользователя"
            });

        }
    }


}