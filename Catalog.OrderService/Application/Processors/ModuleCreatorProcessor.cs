using System.Data.Common;
using System.Reflection.Metadata;
using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.Entities;
using Catalog.Domain.Enum;
using Catalog.OrderService.Application.Configurations;
using Microsoft.Extensions.Options;

namespace Catalog.OrderService.Application.Processors
{
    public class ModuleCreatorProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public ModuleCreatorProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<ModuleDto, string>> ProcessAsync(CreateModuleDto model, CancellationToken cancellationToken = default)
        {
            var moduleType = await _unitOfWork
                .GetRepository<ModuleType>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == model.ModuleTypeCode,
                        trackingType: TrackingType.Tracking);

            if (moduleType is null)
                return Operation.Error("ModuleType not found!");

            var parameterTypes = await _unitOfWork
                .GetRepository<ParameterType>()
                .GetAllAsync(
                    predicate: x=>(model.textParameters != null && model.textParameters.Select(p => p.TypeCode).Contains(x.Code))
                    || (model.numericParameters != null && model.numericParameters.Select(p => p.TypeCode).Contains(x.Code)),
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
                requaredParameters: _applicationConfiguration.Value.ModuleRequaredParameters,
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
