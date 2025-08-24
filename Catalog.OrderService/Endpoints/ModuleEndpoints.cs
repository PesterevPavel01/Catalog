using Asp.Versioning;
using Calabonga.AspNetCore.AppDefinitions;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Domain.Entities;
using Catalog.OrderService.Application.Processors;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.OrderService.Endpoints
{
    public class ModuleEndpoints : AppDefinition
    {
        public override void ConfigureApplication(WebApplication app)
            => app.MapModuleEndpoints();
    }

    internal static class ModuleComplectationEndpointDefinitionExtensions
    {
        public static async Task MapModuleEndpoints(this IEndpointRouteBuilder routes)
        {
            var versionSet = routes.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(2, 0))
                .ReportApiVersions()
                .Build();

            var group = routes.MapGroup("/api/v{version:apiVersion}/Module/")
                .WithApiVersionSet(versionSet)
                .HasApiVersion(new ApiVersion(2, 0));

            group.MapPost("Create", async (
                    IUnitOfWork unitOfWork,
                    [FromBody] CreateModuleDto model,
                    [FromServices] ModuleCreatorProcessor creatorProcessor,
                    CancellationToken cancellationToken) =>
            {
                var moduleType = (await unitOfWork.GetRepository<ModuleType>().GetAllAsync(trackingType: TrackingType.NoTracking)).FirstOrDefault(x => x.Code == model.ModuleTypeCode);

                if (moduleType is null)
                {
                    var moduleTypeResult = ModuleType.Create(model.ModuleType, model.ModuleTypeCode);

                    if (!moduleTypeResult.Ok)
                        return Results.BadRequest(moduleTypeResult.Error);

                    await unitOfWork.GetRepository<ModuleType>().InsertAsync(moduleTypeResult.Result, cancellationToken);

                    await unitOfWork.SaveChangesAsync();

                    if (unitOfWork.Result.Exception is not null)
                        return Results.BadRequest(unitOfWork.Result.Exception.Message);

                    moduleType = moduleTypeResult.Result;
                }

                var result = await creatorProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Result);
            })
                //.RequireAuthorization("Administrator")
                .Produces(200)
                .ProducesProblem(401)
                .WithName("GetModuleCreateEndpoint")
                .WithOpenApi(operation => new(operation)
                {
                    Summary = "Создание модуля.",
                    Description = @"{
  ""moduleType"": ""Фасад"",
  ""moduleTypeCode"": ""00000000FSD"",
  ""numericParameters"": [
    {
      ""type"": ""Ширина"",
      ""typeCode"": ""0000000WDHT"",
      ""value"": 200
    },
    {
      ""type"": ""Длина"",
      ""typeCode"": ""000000LNGHT"",
      ""value"": 400
    }
  ],
  ""textParameters"": [
    {
      ""type"": ""Тон"",
      ""typeCode"": ""000000000TN"",
      ""value"": ""Матовый""
    }
  ]
}"
                });

            group.MapPost("AddComponent", async (
                [FromBody] ModuleComplectationDto model,
                ModuleComplectationProcessor moduleComplectationProcessor,
                CancellationToken cancellationToken) =>
            {
                var result = await moduleComplectationProcessor.ProcessAsync(model, cancellationToken);

                if (!result.Ok)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result);
            })
            //.RequireAuthorization("Administrator")
            .Produces(200)
            .ProducesProblem(401)
            .WithName("GetModuleTestEndpoint")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Добавление компонента.",
                Description = @"
                {
                  ""moduleCode"": ""35a56634-cc84-4aef-94a2-5e9da07a16d0"",
                  ""componentCode"": ""00080196471""
                }"
            });
        }
    }
}
