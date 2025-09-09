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
    public class ComponentAddTextParameterProcessor
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<ApplicationConfiguration> _applicationConfiguration;

        public ComponentAddTextParameterProcessor(IUnitOfWork unitOfWork, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _unitOfWork = unitOfWork;
            _applicationConfiguration = applicationConfiguration;
        }

        public async Task<Operation<ComponentDto, string>> ProcessAsync(ComponentAddTextParameterDto model, CancellationToken cancellationToken)
        {
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

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            await _unitOfWork.GetRepository<ComponentTextParameter>().InsertAsync(textParameters, cancellationToken);

            var component = await _unitOfWork
                .GetRepository<Component>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Code == model.ComponentCode,
                        trackingType: TrackingType.Tracking,
                        include: Component.IncludeRequaredField()
                    );

            if (component is null)
                return Operation.Error($"Component not found! Code: {model.ComponentCode}");

            var insertResult = component.AddTextParameters(
                textParameters,
                componentMultipleParameters: _applicationConfiguration.Value.ComponentMultipleParameters,
                componentRequaredParameters: _applicationConfiguration.Value.ComponentRequaredParameters,
                customComponentRequaredParameters: _applicationConfiguration.Value.CustomComponentRequaredParameters);

            if (!insertResult.Ok)
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
