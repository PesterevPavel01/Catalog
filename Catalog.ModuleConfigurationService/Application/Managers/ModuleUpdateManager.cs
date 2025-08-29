using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Module;
using Catalog.ModuleConfigurationService.Application.Processors;

namespace Catalog.ModuleConfigurationService.Application.Managers
{
    public sealed class ModuleUpdateManager
    {
        private readonly ModuleDataPurgeProcessor _dataPurgeProcessor;
        private readonly ModuleUpdaterProcessor _moduleUpdaterProcessor;
        private readonly ModuleComplectationProcessor _complectationProcessor;

        public ModuleUpdateManager(ModuleDataPurgeProcessor dataPurgeProcessor, ModuleUpdaterProcessor moduleUpdaterProcessor, ModuleComplectationProcessor complectationProcessor)
        {
            _complectationProcessor = complectationProcessor;
            _dataPurgeProcessor = dataPurgeProcessor;
            _moduleUpdaterProcessor = moduleUpdaterProcessor;
        }

        public async Task<Operation<ModuleDto, string>> UpdateAsync(UpdateModuleDto model, CancellationToken cancellationToken)
        {
            var operationResult = await _dataPurgeProcessor.ProcessAsync(model.ModuleCode);

            if (!operationResult.Ok)
                return Operation.Error(operationResult.Error);

            operationResult = await _moduleUpdaterProcessor.ProcessAsync(model, cancellationToken);

            if (!operationResult.Ok)
                return Operation.Error(operationResult.Error);

            foreach (var component in model.Components)
            {
                operationResult = await _complectationProcessor
                .ProcessAsync(
                    new()
                    {
                        ComponentCode = component.ComponentCode,
                        ModuleCode = model.ModuleCode,
                    },
                cancellationToken);

                if (!operationResult.Ok)
                    return Operation.Error(operationResult.Error);
            }

            return operationResult.Result;
        }
    }
}
