using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Domain.Entities;
using Catalog.Domain.Enum;
using Microsoft.Extensions.Options;
using Catalog.ModuleConfigurationService.Application.Configurations;

namespace Catalog.ModuleConfigurationService.Application.Processors
{
    public class ModuleUpdaterProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public ModuleUpdaterProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<ModuleDto, string>> ProcessAsync(UpdateModuleDto model, CancellationToken cancellationToken = default)
        {
            var module = await _unitOfWork
                .GetRepository<Module>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.ModuleCode,
                    trackingType: TrackingType.Tracking,
                    include: Module.IncludeRequaredField());

            if (module is null)
                return Operation.Error($"Module not found! Code: {model.ModuleCode}");

            var parameterTypes = await _unitOfWork
                .GetRepository<ParameterType>()
                .GetAllAsync(
                    predicate: x => model.textParameters != null && model.textParameters.Select(p => p.TypeCode).Contains(x.Code)
                    || model.numericParameters != null && model.numericParameters.Select(p => p.TypeCode).Contains(x.Code),
                    trackingType: TrackingType.Tracking);

            //создаю параметры
            List<ModuleTextParameter> textParameters = [];
            List<ModuleNumericParameter> numericParameters = [];

            foreach (var textParameter in model.textParameters)
            {
                var parameterType = parameterTypes.FirstOrDefault(x => x.Code == textParameter.TypeCode);

                if (parameterType is null)
                {
                    var parameterTypeCreateResult = ParameterType.Create(textParameter.Type, textParameter.TypeCode, ParameterValueType.Text);

                    if (!parameterTypeCreateResult.Ok)
                        return Operation.Error(parameterTypeCreateResult.Error);

                    parameterType = parameterTypeCreateResult.Result;
                }

                var createParameterResult = ModuleTextParameter.Create(textParameter.Value, parameterType);

                if (!createParameterResult.Ok)
                    return Operation.Error(createParameterResult.Error);

                textParameters.Add(createParameterResult.Result);
            }

            foreach (var numericParameter in model.numericParameters)
            {
                var parameterType = parameterTypes.FirstOrDefault(x => x.Code == numericParameter.TypeCode);

                if (parameterType is null)
                {
                    var parameterTypeCreateResult = ParameterType.Create(numericParameter.Type, numericParameter.TypeCode, ParameterValueType.Numeric);

                    if (!parameterTypeCreateResult.Ok)
                        return Operation.Error(parameterTypeCreateResult.Error);

                    parameterType = parameterTypeCreateResult.Result;
                }

                var createParameterResult = ModuleNumericParameter.Create(numericParameter.Value, parameterType);

                if (!createParameterResult.Ok)
                    return Operation.Error(createParameterResult.Error);

                numericParameters.Add(createParameterResult.Result);
            }

            var updateOperationResult = module.Update
            (
                requaredParameters: _applicationConfiguration.Value.ModuleRequaredParameters,
                numericParameters: numericParameters,
                textParameters: textParameters
            );

            if (!updateOperationResult.Ok)
                return Operation.Error(updateOperationResult.Error);

            var insertResult = await _unitOfWork.SaveChangesAsync();

            return module.ConvertToDto();
        }
    }
}
