using Calabonga.OperationResults;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Domain.Entities;
using Newtonsoft.Json;

namespace Catalog.ComponentCompatibilityValidator
{
    public class CompatibilityValidator
    {
        private List<ComponentCompatibilityRule> _componentCompatibilityRules = null!;

        public void SetComponentCompatibilityRules(List<ComponentCompatibilityRule> componentCompatibilityRules) 
        {
            _componentCompatibilityRules = componentCompatibilityRules;
        }

        public Operation<bool, string> Validate(Component firstComponent, Component secondComponent)
        {
            var ValidationResult = ValidateComponent(firstComponent, secondComponent);
            
            if (!ValidationResult.Ok)
                return ValidationResult;

            ValidationResult = ValidateComponent(secondComponent, firstComponent);
                
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

            var textParameterTypes = targetComponent.ComponentTextParameters
                .GroupBy(x => x.ParameterType.Id)
                .Select(r => 
                    new {
                            r.First().ParameterType.Code
                    }).ToArray();

            foreach (var textParameterType in textParameterTypes)
            {
                var targetParameters = targetComponent.ComponentTextParameters.Where(x => x.ParameterType.Code == textParameterType.Code).ToList();

                var result = CheckTextParameter(targetParameters, targetComponent, existingComponent);

                if (!result.Ok)
                    return result;
            }

            return true;
        }

        private Operation<bool, string> CheckTextParameter(List<ComponentTextParameter> targetParameters, Component targetComponent, Component componentToValidate) 
        {
            var parameterRules = _componentCompatibilityRules.FirstOrDefault(x => x.ComponentParameter == targetParameters.First().ParameterType.Title.Value);

            if (parameterRules is null)
                return true;

            foreach (var parameterRule in parameterRules.Dependencies) {

                if (parameterRule.TargetComponentTypes is not null && !parameterRule.TargetComponentTypes.Contains(componentToValidate.ComponentType.Title.Value))
                    continue;

                var validationParameters = componentToValidate.ComponentTextParameters
                    .Where(x => x.ParameterType.Title.Value == parameterRule.Parameter ).ToList();
                
                if (validationParameters is null || validationParameters.Count == 0)
                    continue;

                switch (parameterRule.ComparisonRule)
                {
                    case "Contains":
                        if (!validationParameters.Select(x => x.Value).Intersect(targetParameters.Select(x => x.Value)).Any())
                            return Operation.Error($"Не выполняется условие для свойства '{targetParameters.First().ParameterType.Title.Value}' компонента '{targetComponent.ComponentType.Title.Value}' (код: '{targetComponent.Code}')! Ни одно значение {JsonConvert.SerializeObject(targetParameters.Select(x => x.Value.Value), Formatting.Indented)} его свойства '{targetParameters.First().ParameterType.Title.Value}' не найдено в списке разрешенных значений({JsonConvert.SerializeObject(validationParameters.Select(x => x.Value.Value), Formatting.Indented)}) у компонента '{componentToValidate.ComponentType.Title.Value}' (код: {componentToValidate.Code}), ");
                        break;
                    case "NoContains":
                        if (validationParameters.Select(x => x.Value).Intersect(targetParameters.Select(x => x.Value)).Any())
                            return Operation.Error($"Не выполняется условие для свойства '{targetParameters.First().ParameterType.Title.Value}' компонента '{targetComponent.ComponentType.Title.Value}' (код: '{targetComponent.Code}')! Есть значение '{JsonConvert.SerializeObject(targetParameters.Select(x => x.Value.Value), Formatting.Indented)}' его свойства '{targetParameters.First().ParameterType.Title.Value}', которое найдено в списке значений({JsonConvert.SerializeObject(validationParameters.Select(x => x.Value.Value), Formatting.Indented)}) у компонента '{componentToValidate.ComponentType.Title.Value}' (код: '{componentToValidate.Code}'), ");
                        break;
                    default:
                        return Operation.Error($"ComparisonRule not found {parameterRule.ComparisonRule} (ComponentParameter: '{targetParameters.First().ParameterType.Title.Value}'. Type parameter: '{targetParameters.First().GetType().Name}')");
                }
            }
            return true;
        }

        private Operation<bool, string> CheckNumericParameter(ComponentNumericParameter targetParameter, Component targetComponent, Component componentToValidate)
        {
            var parameterRules = _componentCompatibilityRules.FirstOrDefault(x => x.ComponentParameter == targetParameter.ParameterType.Title.Value);

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
                        if (validationParameter.Value == (targetParameter.Value))
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
