using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Request;
using Catalog.Contracts.Response;
using Catalog.Domain.Entities;
using Catalog.Domain.Enum;
using Rebus;
using Rebus.Bus;

namespace Catalog.ModuleConfigurationService.Application.Processors
{
    public class ModuleUpdaterProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IModuleParametersValidator _moduleParametersValidator;

        public ModuleUpdaterProcessor(
            IModuleParametersValidator moduleParametersValidator,
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _moduleParametersValidator = moduleParametersValidator;
        }

        public async Task<Operation<ModuleDto, string>> ProcessAsync(UpdateModuleDto model, CancellationToken cancellationToken = default)
        {
            var module = await _unitOfWork
                .GetRepository<Module>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.ModuleCode,
                    trackingType: TrackingType.Tracking,
                    include: Module.IncludeRequiredField());

            if (module is null)
                return Operation.Error($"Module not found! Code: {model.ModuleCode}");

            var parameterTypes = await _unitOfWork
                .GetRepository<ParameterType>()
                .GetAllAsync(
                    predicate: x => model.TextParameters != null && model.TextParameters.Select(p => p.TypeCode).Contains(x.Code)
                    || model.NumericParameters != null && model.NumericParameters.Select(p => p.TypeCode).Contains(x.Code),
                    trackingType: TrackingType.Tracking);

            //создаю параметры
            List<ModuleTextParameter> textParameters = [];
            List<ModuleNumericParameter> numericParameters = [];

            foreach (var textParameter in model.TextParameters)
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

            foreach (var numericParameter in model.NumericParameters)
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
                parametersValidator: _moduleParametersValidator,
                numericParameters: numericParameters,
                textParameters: textParameters
            );

            if (!updateOperationResult.Ok)
                return Operation.Error(updateOperationResult.Error);

            var insertResult = await _unitOfWork.SaveChangesAsync();

            return module.ConvertToDto(); ;
        }
    }
}
