using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.ModuleConfigurationService.Application.Processors;

namespace Catalog.ModuleConfigurationService.Application.Managers
{
    public sealed class ModuleCreateManager
    {
        private readonly ModuleCreatorProcessor _moduleCreator;
        private readonly ModuleUpdaterProcessor _moduleUpdater;
        private readonly ModuleComplectationProcessor _complectationProcessor;
        private readonly IUnitOfWork _unitOfWork;

        public ModuleCreateManager(IUnitOfWork unitOfWork, ModuleCreatorProcessor moduleCreator, ModuleUpdaterProcessor moduleUpdaterProcessor, ModuleComplectationProcessor complectationProcessor)
        {
            _unitOfWork = unitOfWork;
            _complectationProcessor = complectationProcessor;
            _moduleUpdater = moduleUpdaterProcessor;
            _moduleCreator = moduleCreator;
        }

        public async Task<Operation<ModuleDto, string>> CreateAsync(CreateModuleDto model, CancellationToken cancellationToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var operationResult = await _moduleCreator.ProcessAsync(model, cancellationToken);

            if (operationResult.Ok)
                foreach (var component in model.Components)
                {
                    operationResult = await _complectationProcessor
                    .ProcessAsync(
                        new()
                        {
                            ComponentCode = component.ComponentCode,
                            ModuleCode = operationResult.Result.ModuleCode,
                        },
                    cancellationToken);

                    if (!operationResult.Ok)
                        break;

                }

            if (!operationResult.Ok)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Operation.Error(operationResult.Error);
            }

            await transaction.CommitAsync(cancellationToken);

            return operationResult.Result;
        }
    }
}
