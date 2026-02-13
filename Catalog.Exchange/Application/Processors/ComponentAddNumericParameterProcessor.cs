using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.Entities;
using Catalog.Domain.Enum;
using Microsoft.Extensions.Options;

namespace Catalog.ExchangeService.Application.Processors
{
    public sealed class ComponentAddNumericParameterProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ComponentConfiguration> _componentConfiguration;

        public ComponentAddNumericParameterProcessor(IUnitOfWork unitOfWork, IOptions<ComponentConfiguration> options)
        {
            _unitOfWork = unitOfWork;
            _componentConfiguration = options;
        }

        public async Task<Operation<ComponentDto, string>> ProcessAsync(ComponentAddNumericParameterDto model, CancellationToken cancellationToken)
        {
            var parameterTypes = await _unitOfWork
                    .GetRepository<ParameterType>()
                    .GetAllAsync(
                        predicate: x => model.NumericParameters.Select(p => p.TypeCode).Contains(x.Code),
                        trackingType: TrackingType.Tracking);

            List<ComponentNumericParameter> numericParameters = [];

            foreach (var parameter in model.NumericParameters)
            {
                var parameterType = parameterTypes.FirstOrDefault(x => x.Code == parameter.TypeCode);

                if (parameterType is null)
                {
                    var parameterTypeCreateResult = ParameterType.Create(parameter.Type, parameter.TypeCode, ParameterValueType.Numeric);

                    if (!parameterTypeCreateResult.Ok)
                        return Operation.Error(parameterTypeCreateResult.Error);

                    parameterType = parameterTypeCreateResult.Result;
                }

                var componentNumericParameter = ComponentNumericParameter
                        .Create(parameter.Value, parameterType);

                if (!componentNumericParameter.Ok)
                    return Operation.Error(componentNumericParameter.Error);

                numericParameters
                    .Add(componentNumericParameter.Result);
            }

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.GetRepository<ComponentNumericParameter>().InsertAsync(numericParameters, cancellationToken);

            var component = await _unitOfWork
                .GetRepository<Component>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == model.ComponentCode,
                        trackingType: TrackingType.Tracking,
                        include: Component.IncludeRequiredField());

            if (component is null)
                return Operation.Error($"Component not found! Code: {model.ComponentCode}");

            var insertResult = component.AddNumericParameters(
                numericParameters,
                componentMultipleParameters: [.. _componentConfiguration.Value.ComponentMultipleParameters]);
                
            if(!insertResult.Ok)
                return Operation.Error(insertResult.Error);

            var result = await _unitOfWork.SaveChangesAsync();

            if (_unitOfWork.Result.Exception is not null)
            {
                await transaction.RollbackAsync();
                return Operation.Error(_unitOfWork.Result.Exception.Message);
            }

            await transaction.CommitAsync(cancellationToken);

            return component.ConvertToDto();
        }
    }
}
