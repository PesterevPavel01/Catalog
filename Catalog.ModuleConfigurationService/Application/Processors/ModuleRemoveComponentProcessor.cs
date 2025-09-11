using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Domain.Entities;

namespace Catalog.ModuleConfigurationService.Application.Processors
{
    public class ModuleRemoveComponentProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public ModuleRemoveComponentProcessor(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Operation<ModuleDto, string>> ProcessAsync(ModuleComplectationDto model, CancellationToken cancellationToken = default)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            var module = await _unitOfWork
                .GetRepository<Module>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.ModuleCode,
                    trackingType: TrackingType.Tracking,
                    include: Module.IncludeRequiredField()
                );

            if (module is null)
                return Operation.Error("Module not found!");

            var targetComponent = module.Components.FirstOrDefault(x => x.Code == model.ComponentCode);

            if (targetComponent is null)
                return Operation.Error($"Component with code \"{model.ComponentCode}\" was not found in the module!");

            var removeComponentResult = module.RemoveComponent(targetComponent);

            if (!removeComponentResult.Ok)
                return Operation.Error(removeComponentResult.Error);

            var updateResult = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync();

                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            transaction.Commit();

            return module.ConvertToDto();
        }
    }
}
