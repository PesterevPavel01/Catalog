using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Catalog.Contracts.Dto;
using Catalog.Contracts.Events;
using Catalog.ExchangeService.Application.Processors;
using Microsoft.AspNetCore.Mvc;
using Rebus.Bus;

namespace Catalog.Web.Endpoints
{
    public class ComponentEndpoint: AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapComponentEndpoints();
    }
    internal static class ComponentEndpointDefinitionExtensions
    {
        public static async Task MapComponentEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/Component/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("Create", async (
                [FromBody] ComponentDto model,
                [FromServices] IBus bus,
                ComponentCreatorProcessor componentCreatorProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await componentCreatorProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                if (result.Ok)
                    await bus.Publish(new ComponentCreatedEvent([.. result.Result.Select( x => x.ComponentCode)]));

                return Results.Ok(result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("ModuleCreateEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Создание компонента",
                Description = @"Пример запроса:<br>
                {
                  ""componentCode"": ""00080184744"",
                  ""componentTitle"": ""Нестандартная"",
                  ""componentTypeCode"": ""0000000FRZK"",
                  ""componentTypeTitle"": ""Фрезеровка"",
                  ""numericParameters"": [
                    {
                      ""type"": ""Минимальная ширина"",
                      ""typeCode"": ""00000MWDHT"",
                      ""value"": 150
                    },
                    {
                      ""type"": ""Тип фрезеровки"",
                      ""typeCode"": ""00000TPFRZ"",
                      ""value"": 1
                    }
                  ]
                }<br><br>
                {
                  ""componentCode"": ""00080196471"",
                  ""componentTitle"": ""Пленка матовая Венге рифленый 30209-22"",
                  ""componentTypeCode"": ""00080195637"",
                  ""componentTypeTitle"": ""ПЛЕНКА ПВХ"",
                  ""textParameters"": [
                    {
                      ""type"": ""Тон"",
                      ""typeCode"": ""00000000TN"",
                      ""value"": ""Матовая""
                    }
                  ],
                  ""numericParameters"": [
                    {
                      ""type"": ""Тип фрезеровки"",
                      ""typeCode"": ""00000TPFRZ"",
                      ""value"": 1
                    }
                  ]
                }"
            });

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
