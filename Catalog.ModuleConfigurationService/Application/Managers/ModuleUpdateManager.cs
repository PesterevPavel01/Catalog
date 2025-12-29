using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Request;
using Catalog.Contracts.Response;
using Catalog.ModuleConfigurationService.Application.Processors;
using Rebus;
using Rebus.Bus;

namespace Catalog.ModuleConfigurationService.Application.Managers
{
    public sealed class ModuleUpdateManager
    {

        private readonly ModuleDataPurgeProcessor _dataPurgeProcessor;
        private readonly ModuleUpdaterProcessor _moduleUpdaterProcessor;
        private readonly ModuleComplectationProcessor _complectationProcessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBus _bus;

        public ModuleUpdateManager(IUnitOfWork unitOfWork, ModuleDataPurgeProcessor dataPurgeProcessor, ModuleUpdaterProcessor moduleUpdaterProcessor,      ModuleComplectationProcessor complectationProcessor,
            IBus bus)
        {
            _unitOfWork = unitOfWork;
            _complectationProcessor = complectationProcessor;
            _dataPurgeProcessor = dataPurgeProcessor;
            _moduleUpdaterProcessor = moduleUpdaterProcessor;
            _bus = bus;
        }

        public async Task<Operation<ModuleDto, string>> UpdateAsync(UpdateModuleDto model, CancellationToken cancellationToken)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var operationResult = await _dataPurgeProcessor.ProcessAsync(model.ModuleCode, cancellationToken);

            if (operationResult.Ok)
                operationResult = await _moduleUpdaterProcessor.ProcessAsync(model, cancellationToken);

            if (operationResult.Ok)
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
                        break;

                }

            if (!operationResult.Ok) {
                await transaction.RollbackAsync(cancellationToken);
                return Operation.Error(operationResult.Error);
            }

            var orderValidationRequest = new ModuleChangePermissionRequest(operationResult.Result);

            var orderValidationResponse = await _bus.SendRequest<ModuleChangePermissionResponse>(
                orderValidationRequest,
                timeout: TimeSpan.FromSeconds(15),
                externalCancellationToken: cancellationToken);

            if (!orderValidationResponse.Result)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Operation.Error(orderValidationResponse.Error ?? "Unknown error occurred");
            }

            await transaction.CommitAsync(cancellationToken);

            return operationResult.Result;
        }
    }
}
