using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Domain.Entities;

namespace Catalog.ModuleConfigurationService.Application.Processors
{
    public class ModuleDataPurgeProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public ModuleDataPurgeProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<ModuleDto, string>> ProcessAsync(String moduleCode, CancellationToken cancellationToken = default)
        {
            var module = await _unitOfWork
                .GetRepository<Module>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == moduleCode,
                    trackingType: TrackingType.Tracking,
                    include: Module.IncludeRequaredField());

            if (module is null)
                return Operation.Error($"Module not found! Code: {moduleCode}");

            var componentModels = module.Components.Select(x => x.ConvertToDto()).ToArray();

            foreach (var item in componentModels)
            {
                var component = module.Components.FirstOrDefault(x => x.Code == item.ComponentCode);

                if (module is null)
                    return Operation.Error("Internal server error.");

                var operationResult = module.RemoveComponent(component);

                if (!operationResult.Ok)
                    return Operation.Error(operationResult.Error);
            }

            var numericParameterModels = module.ModuleNumericParameters.Select(x => x.ConvertToDto()).ToArray();

            foreach (var item in numericParameterModels)
            {
                var parameter = module.ModuleNumericParameters.FirstOrDefault(x => x.Id == item.GetId());

                if (parameter is null)
                    return Operation.Error("Internal server error.");

                var operationResult = module.RemoveNumericParameter(parameter);
                
                if (!operationResult.Ok)
                    return Operation.Error(operationResult.Error);
            }

            var textParameterModels = module.ModuleTextParameters.Select(x => x.ConvertToDto()).ToArray();

            foreach (var item in textParameterModels)
            {
                var parameter = module.ModuleTextParameters.FirstOrDefault(x => x.Id == item.GetId());

                if (parameter is null)
                    return Operation.Error("Internal server error.");

                var operationResult = module.RemoveTextParameter(parameter);

                if (!operationResult.Ok)
                    return Operation.Error(operationResult.Error);
            }

            await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
                return Operation.Error(_unitOfWork.Result.Exception.Message);

            return module.ConvertToDto();
        }
    }
}
