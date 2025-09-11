using Calabonga.OperationResults;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Catalog.ComponentParametersValidator
{
    public class ComponentParametersValidator: IComponentParametersValidator
    {
        private readonly IEnumerable<ComponentRequiredRarameter> _requiredParameters;
        private readonly IEnumerable<ComponentRequiredRarameter> _customComponentRequiredParameters;

        public ComponentParametersValidator(IOptions<ComponentConfiguration> options)
        {
            _requiredParameters = [.. options.Value.ComponentRequiredParameters];
            _customComponentRequiredParameters = [.. options.Value.CustomComponentRequiredParameters];
        }

        public Operation<bool, string> Validate(Component component)
        {
            if (_requiredParameters is null)
                return Operation.Error("RequiredParameters not found");

            if (_customComponentRequiredParameters is null)
                return Operation.Error("CustomComponentRequiredParameters not found");
            
            var checkResult = CheckParameters(component, _requiredParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            if (!component.IsCustom)
                return true;

            checkResult = CheckParameters(component, _customComponentRequiredParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            return true;
        }

        private Operation<bool, string> CheckParameters(Component component, IEnumerable<ComponentRequiredRarameter> requiredParameters)
        {
            var currentRequiredParameters = requiredParameters.FirstOrDefault(x => x.ComponentType == component.ComponentType.Title.Value && x.ComponentTitle is null);

            if (currentRequiredParameters is not null)
            {
                if (currentRequiredParameters is null)
                    return true;

                if (component.ComponentNumericParameters is not null)
                {
                    var parameter = currentRequiredParameters.Parameters
                        .FirstOrDefault(x => component.ComponentNumericParameters.Select(x => x.ParameterType.Title.Value).Contains(x));

                    if (parameter is not null)
                        return true;
                }

                if (component.ComponentNumericParameters is not null)
                {
                    var parameter = currentRequiredParameters.Parameters
                        .FirstOrDefault(x => component.ComponentTextParameters.Select(x => x.ParameterType.Title.Value).Contains(x));

                    if (parameter is not null)
                        return true;
                }

                return Operation.Error("Validation failed");
            }

            return true;
        }
    }
}
