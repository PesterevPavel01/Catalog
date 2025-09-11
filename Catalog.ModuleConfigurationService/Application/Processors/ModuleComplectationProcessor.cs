using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.ComponentCompatibilityValidator;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Catalog.ModuleCompositionValidator;
using Microsoft.Extensions.Options;

namespace Catalog.ModuleConfigurationService.Application.Processors
{
    public class ModuleComplectationProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ModuleConfiguration> _applicationConfiguration;
        private readonly CompositionValidator _moduleCompositionValidator;
        private readonly CompatibilityValidator _componentCompatibilityValidator;
        private readonly IModuleParametersValidator _moduleParametersValidator;

        public ModuleComplectationProcessor(
            IUnitOfWork unitOfWork, 
            IOptions<ModuleConfiguration> applicationConfiguration,
            CompatibilityValidator compatibilityValidator,
            CompositionValidator moduleCompositionValidator,
            IModuleParametersValidator moduleParametersValidator)
        {
            _applicationConfiguration = applicationConfiguration;
            _unitOfWork = unitOfWork;
            _componentCompatibilityValidator = compatibilityValidator;
            _componentCompatibilityValidator.SetComponentCompatibilityRules(_applicationConfiguration.Value.ComponentCompatibilityRules);
            _moduleCompositionValidator = moduleCompositionValidator;
            _moduleCompositionValidator.SetModuleCompositionRules(_applicationConfiguration.Value.ModuleCompositionRules);
            _moduleParametersValidator = moduleParametersValidator;
        }

        public async Task<Operation<ModuleDto, string>> ProcessAsync(ModuleComplectationDto model, CancellationToken cancellationToken = default)
        {
            var moduleRepository = _unitOfWork.GetRepository<Module>();

            var module = await moduleRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.ModuleCode,
                    trackingType: TrackingType.Tracking,
                    include: Module.IncludeRequiredField());

            if (module is null)
                return Operation.Error("Module not found!"); 

            if(module.Components.FirstOrDefault(x => x.Code == model.ComponentCode) is not null)
                return Operation.Error("Component has already been added to the selected module!");

            var componentRepository = _unitOfWork.GetRepository<Component>();

            var component = await componentRepository
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.ComponentCode,
                    trackingType: TrackingType.Tracking,
                    include: Component.IncludeRequiredField());

            if (component is null)
                return Operation.Error("Component not found!");

            var validationResult = _moduleCompositionValidator.Validate(module, component);

            if (!validationResult.Ok)
                return Operation.Error(validationResult.Error);

            if (module.Components.Count > 0)
            {
                var result = await IsComponentCompatibleWithModule(module, component, cancellationToken);

                if (!result.Ok)
                    return Operation.Error(result.Error);
            }

            var createModuleResult = module.AddComponent(component, _moduleParametersValidator);

            if(!createModuleResult.Ok) 
                return createModuleResult;

            await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
                return Operation.Error(_unitOfWork.Result.Exception.Message);

            return module.ConvertToDto();
        }

        private async Task<Operation<bool, string>> IsComponentCompatibleWithModule(Module module, Component component, CancellationToken cancellationToken)
        {
            var componentRepository = _unitOfWork.GetRepository<Component>();
            
            var existingComponents = await componentRepository
                .GetAllAsync(
                    predicate: x => x.Modules.FirstOrDefault(x => x.Id == x.Id) != null,
                    trackingType: TrackingType.Tracking,
                    include: Component.IncludeRequiredField());

            foreach (var existingComponent in existingComponents)
            {
                var compabilityResult = _componentCompatibilityValidator.Validate(existingComponent, component);

                if (!compabilityResult.Ok)
                    return Operation.Error(compabilityResult.Error);

            }

            return true;
        }
    }
}
