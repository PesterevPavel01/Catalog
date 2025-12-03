using Calabonga.OperationResults;
using Calabonga.UnitOfWork;
using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Enum;
using Microsoft.Extensions.Options;

namespace Catalog.ExchangeService.Application.Processors
{
    public class TextParametersReplacer
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IComponentParametersValidator _parametersValidator;
        private readonly List<String> _componentMultipleParameters;

        public TextParametersReplacer(IUnitOfWork unitOfWork, IComponentParametersValidator parametersValidator, IOptions<ComponentConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _parametersValidator = parametersValidator;
            _componentMultipleParameters = [.. applicationConfiguration.Value.ComponentMultipleParameters];
        }

        public async Task<Operation<ComponentDto, string>> ProcessAsync(ComponentDto model, CancellationToken cancellationToken)
        {
            var component = await _unitOfWork
                    .GetRepository<Component>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == model.ComponentCode,
                        include: Component.IncludeRequiredField(),
                        trackingType: TrackingType.Tracking);

            if (component is null)
                return Operation.Error($"Component not found! Code: {model.ComponentCode}");

            var parameterTypes = await _unitOfWork
                    .GetRepository<ParameterType>()
                    .GetAllAsync(
                        predicate: x => model.TextParameters.Select(p => p.TypeCode).Contains(x.Code),
                        trackingType: TrackingType.Tracking);

            List<ComponentTextParameter> textParameters = [];

            foreach (var parameter in model.TextParameters)
            {
                var parameterType = parameterTypes.FirstOrDefault(x => x.Code == parameter.TypeCode);

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

            var existingTextParameters = await _unitOfWork
                    .GetRepository<ComponentTextParameter>()
                    .GetAllAsync(
                        predicate: x => x.ComponentId == component.Id,
                        trackingType: TrackingType.Tracking);

            if(existingTextParameters.Any())
                _unitOfWork.GetRepository<ComponentTextParameter>().Delete(existingTextParameters);

            var replaceResult = component.ReplaceTextParameters(_componentMultipleParameters, _parametersValidator, textParameters);

            if (!replaceResult.Ok)
                return Operation.Error(replaceResult.Error);

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.GetRepository<ComponentTextParameter>().InsertAsync(textParameters, cancellationToken);

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
