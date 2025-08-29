using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.Entities;
using Catalog.Domain.Enum;
using Catalog.ExchangeService.Application.Configurations;
using Microsoft.Extensions.Options;

namespace Catalog.ExchangeService.Application.Processors
{
    public class ComponentCreatorProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly List<ComponentRequaredRarameter> _componentRequaredParameters;
        private readonly List<ComponentRequaredRarameter> _customComponentRequaredParameter;
        private readonly List<String> _componentMultipleParameters;
        public ComponentCreatorProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;

            _componentRequaredParameters = applicationConfiguration.Value.ComponentRequaredParameters;

            _componentMultipleParameters = applicationConfiguration.Value.ComponentMultipleParameters;

            _customComponentRequaredParameter = applicationConfiguration.Value.CustomComponentRequaredParameters;

        }

        /// <summary>
        /// Метод для создания нового компонента
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        
        public async Task<Operation<List<ComponentDto>, string>> ProcessAsync(ComponentDto model,CancellationToken cancellationToken)
        {
            List<ComponentTextParameter> textParameters = [];

            List<ComponentNumericParameter> numericParameters = [];

            List<String> parameterTypes = [];

            parameterTypes = model.NumericParameters is not null ? [.. model.NumericParameters.Select(x => x.TypeCode)] : [];

            parameterTypes.AddRange(model.TextParameters is not null ? [.. model.TextParameters.Select(x => x.TypeCode)] : []);

            var parameterTypesEntity = await _unitOfWork
                    .GetRepository<ParameterType>()
                    .GetAllAsync(
                        predicate: x => parameterTypes.Contains(x.Code),
                        trackingType: TrackingType.Tracking);

            foreach (var parameter in model.TextParameters)
            {
                var parameterType = parameterTypesEntity.FirstOrDefault(x => x.Code == parameter.TypeCode);

                if (parameterType is null)
                {
                    var parameterTypeCreateResult = ParameterType.Create(parameter.Type, parameter.TypeCode, ParameterValueType.Text);

                    if (!parameterTypeCreateResult.Ok)
                        return Operation.Error(parameterTypeCreateResult.Error);

                    parameterType = parameterTypeCreateResult.Result;
                }

                var componentTextParameter = ComponentTextParameter
                        .Create(parameter.Value, parameterType);

                if (!componentTextParameter.Ok)
                    return Operation.Error(componentTextParameter.Error);

                textParameters
                    .Add(componentTextParameter.Result);
            }

            foreach (var parameter in model.NumericParameters)
            {
                var parameterType = parameterTypesEntity.FirstOrDefault(x => x.Code == parameter.TypeCode);

                if (parameterType is null)
                {
                    var parameterTypeCreateResult = ParameterType.Create(parameter.Type, parameter.TypeCode, ParameterValueType.Numeric);

                    if (!parameterTypeCreateResult.Ok)
                        return Operation.Error(parameterTypeCreateResult.Error);

                    parameterType = parameterTypeCreateResult.Result;
                }

                var numericParameterCreateResult = ComponentNumericParameter
                        .Create(parameter.Value, parameterType);

                if (!numericParameterCreateResult.Ok)
                    return Operation.Error(numericParameterCreateResult.Error);

                numericParameters
                    .Add(numericParameterCreateResult.Result);
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.GetRepository<ComponentTextParameter>().InsertAsync(textParameters, cancellationToken);

            await _unitOfWork.GetRepository<ComponentNumericParameter>().InsertAsync(numericParameters, cancellationToken);

            var componentType = await _unitOfWork
                .GetRepository<ComponentType>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == model.ComponentTypeCode,
                        trackingType: TrackingType.Tracking);

            if (componentType is null)
            {
                var componentTypeCreatedResult = ComponentType.Create(model.ComponentTypeTitle, model.ComponentTypeCode);

                if (!componentTypeCreatedResult.Ok)
                    return Operation.Error(componentTypeCreatedResult.Error);

                componentType = componentTypeCreatedResult.Result;

                await _unitOfWork
                    .GetRepository<ComponentType>()
                        .InsertAsync(componentType, cancellationToken);
            }

            var componentCreateResult = Component
                .Create(
                    title: model.ComponentTitle,
                    code: model.ComponentCode, 
                    componentType: componentType, 
                    componentMultiplyParameters: _componentMultipleParameters,
                    requaredParameters: _componentRequaredParameters,
                    customComponentRequaredParameters: _customComponentRequaredParameter,
                    textParameters,
                    numericParameters);

            if (!componentCreateResult.Ok)
                return Operation.Error(componentCreateResult.Error);

            await _unitOfWork.GetRepository<Component>().InsertAsync(componentCreateResult.Result, cancellationToken);

            var insertResult = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync();
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            await transaction.CommitAsync(cancellationToken);

            return new List<ComponentDto>(){ componentCreateResult.Result.ConvertToDto() };
        }
    }
}
