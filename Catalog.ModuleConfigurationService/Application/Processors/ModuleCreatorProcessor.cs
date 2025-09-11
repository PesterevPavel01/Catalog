using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Enum;
using Microsoft.Extensions.Options;

namespace Catalog.ModuleConfigurationService.Application.Processors
{
    public class ModuleCreatorProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IModuleParametersValidator _moduleParametersValidator;

        public ModuleCreatorProcessor(IUnitOfWork unitOfWork, IOptions<ModuleConfiguration> applicationConfiguration, IModuleParametersValidator moduleCreationValidator)
        {
            _unitOfWork = unitOfWork;
            _moduleParametersValidator = moduleCreationValidator;
        }

        public async Task<Operation<ModuleDto, string>> ProcessAsync(CreateModuleDto model, CancellationToken cancellationToken = default)
        {
            var moduleType = await _unitOfWork.GetRepository<ModuleType>()
                .GetFirstOrDefaultAsync(
                    predicate: x => x.Code == model.ModuleTypeCode, 
                    trackingType: TrackingType.Tracking);

            if (moduleType is null)
            {
                var moduleTypeResult = ModuleType.Create(model.ModuleType, model.ModuleTypeCode);

                if (!moduleTypeResult.Ok)
                    return Operation.Error(moduleTypeResult.Error);

                await _unitOfWork.GetRepository<ModuleType>().InsertAsync(moduleTypeResult.Result, cancellationToken);

                moduleType = moduleTypeResult.Result;
            }

            var parameterTypes = await _unitOfWork
                .GetRepository<ParameterType>()
                .GetAllAsync(
                    predicate: x=>model.textParameters != null && model.textParameters.Select(p => p.TypeCode).Contains(x.Code)
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

            var createModuleResult = Module.Create(
                title: "default",
                code:Guid.NewGuid().ToString(),
                moduleType: moduleType,
                parametersValidator: _moduleParametersValidator,
                numericParameters: numericParameters,
                textParameters: textParameters
            );

            if (!createModuleResult.Ok)
                return Operation.Error(createModuleResult.Error);

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            
            await _unitOfWork.GetRepository<Module>().InsertAsync(createModuleResult.Result, cancellationToken);

            var insertResult = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync();

                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            transaction.Commit();

            return createModuleResult.Result.ConvertToDto();
        }

    }
}
