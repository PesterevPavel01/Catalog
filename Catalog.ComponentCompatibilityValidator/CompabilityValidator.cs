using Calabonga.OperationResults;
using Catalog.ComponentCompabilityValidator.Contracts;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Domain.Entities;

namespace Catalog.ComponentCompatibilityValidator
{
    public class CompabilityValidator
    {
        private List<ComponentCompabilityRule> _componentCompabilityRules = null!;

        public void SetComponentCompabilityRules(List<ComponentCompabilityRule> componentCompabilityRules) 
        {
            _componentCompabilityRules = componentCompabilityRules;
        }

        public Operation<bool, string> Validate(Component firstComponent, Component secondConponent)
        {
            var ValidationResult = ValidateComponent(firstComponent, secondConponent);
            
            if (!ValidationResult.Ok)
                return ValidationResult;

            ValidationResult = ValidateComponent(secondConponent, firstComponent);
                
            return ValidationResult;
        }

        private Operation<bool, string> ValidateComponent(Component existingComponent, Component targetComponent) 
        {
            foreach (var numericParameter in targetComponent.ComponentNumericParameters)
            {
                var result = CheckNumericParameter(numericParameter, targetComponent, existingComponent);

                if (!result.Ok)
                    return result;
            }

            foreach (var textParameter in targetComponent.ComponentTextParameters)
            {
                var result = CheckTextParameter(textParameter, targetComponent, existingComponent);

                if (!result.Ok)
                    return result;
            }

            return true;
        }

        private Operation<bool, string> CheckTextParameter(ComponentTextParameter targetParameter, Component targetComponent, Component componentToValidate) 
        {
            var parameterRules = _componentCompabilityRules.FirstOrDefault(x => x.ComponentParameter == targetParameter.ParameterType.Title.Value);

            if (parameterRules is null)
                return true;

            foreach (var parameterRule in parameterRules.Dependencies) {

                var validationParameters = componentToValidate.ComponentTextParameters
                    .Where(x => 
                        x.ParameterType.Title.Value == parameterRule.Parameter 
                        && (parameterRule.TargetComponentTypes is null || parameterRule.TargetComponentTypes.Contains(targetComponent.ComponentType.Title.Value))).ToList();
                
                if (validationParameters is null || validationParameters.Count == 0)
                    continue;

                switch (parameterRule.ComparisonRule)
                {
                    case "Contains":
                        if (!validationParameters.Select(x => x.Value).Contains(targetParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value} компонента {targetComponent.ComponentType.Title.Value} (код: {targetComponent.Code})! Значение {targetParameter.Value.Value} его свойства {targetParameter.ParameterType.Title.Value} не найдено в списке разрешенных значений({validationParameters.Select(x => x.Value)}) у компонента {componentToValidate.ComponentType.Title} (код: {componentToValidate.Code}), ");
                        break;
                    default:
                        return Operation.Error($"ComparisonRule not found {parameterRule.ComparisonRule} (ComponentParameter: {targetParameter.ParameterType.Title.Value}. Type parameter: {targetParameter.GetType().Name})");
                }
            }
            return true;
        }

        private Operation<bool, string> CheckNumericParameter(ComponentNumericParameter targetParameter, Component targetComponent, Component componentToValidate)
        {
            var parameterRules = _componentCompabilityRules.FirstOrDefault(x => x.ComponentParameter == targetParameter.ParameterType.Title.Value);

            if (parameterRules is null)
                return true;

            foreach (var parameterRule in parameterRules.Dependencies)
            {

                var validationParameter = componentToValidate.ComponentNumericParameters
                    .FirstOrDefault(x =>
                        x.ParameterType.Title.Value == parameterRule.Parameter
                        && (parameterRule.TargetComponentTypes is null || parameterRule.TargetComponentTypes.Contains(targetComponent.ComponentType.Title.Value)));

                if (validationParameter is null)
                    continue;

                switch (parameterRule.ComparisonRule.Trim())
                {
                    case ">":
                        if (!(validationParameter.Value >= targetParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value.ToUpper()} компонента {targetComponent.ComponentType.Title.Value.ToUpper()} (код: {targetComponent.Code})! Значение {targetParameter.Value} его свойства {targetParameter.ParameterType.Title.Value.ToUpper()} должно быть <= значения {validationParameter.Value} свойства {targetParameter.ParameterType.Title.Value.ToUpper()} у компонента {componentToValidate.ComponentType.Title.Value.ToUpper()} (код: {componentToValidate.Code})");
                        break;

                    case "<":
                        if (!(validationParameter.Value <= (targetParameter.Value)))
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value.ToUpper()} компонента {targetComponent.ComponentType.Title.Value.ToUpper()} (код: {targetComponent.Code})! Значение {targetParameter.Value} его свойства {targetParameter.ParameterType.Title.Value.ToUpper()} должно быть >= значения {validationParameter.Value} свойства {targetParameter.ParameterType.Title.Value.ToUpper()} у компонента {componentToValidate.ComponentType.Title.Value.ToUpper()} (код: {componentToValidate.Code})");
                        break;
                    case "=":
                        if (validationParameter.Value != (targetParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value.ToUpper()} компонента {targetComponent.ComponentType.Title.Value.ToUpper()} (код: {targetComponent.Code})! Значение {targetParameter.Value} его свойства {targetParameter.ParameterType.Title.Value.ToUpper()} должно быть равно значению {validationParameter.Value} свойства {targetParameter.ParameterType.Title.Value.ToUpper()} у компонента {componentToValidate.ComponentType.Title.Value.ToUpper()} (код: {componentToValidate.Code})");
                        break;
                    case "!=":
                        if (validationParameter.Value != (targetParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value.ToUpper()} компонента {targetComponent.ComponentType.Title.Value.ToUpper()} (код: {targetComponent.Code})! Значение {targetParameter.Value} его свойства {targetParameter.ParameterType.Title.Value.ToUpper()} должно быть не равно значению {validationParameter.Value} свойства {targetParameter.ParameterType.Title.Value.ToUpper()} у компонента {componentToValidate.ComponentType.Title.Value.ToUpper()} (код: {componentToValidate.Code})");
                        break;
                    default:
                        return Operation.Error($"ComparisonRule not found {parameterRule.ComparisonRule} (ComponentParameter: {targetParameter.ParameterType.Title.Value.ToUpper()}. Type parameter: {targetParameter.GetType().Name})");
                }
            }
            return true;
        }
    }
}
