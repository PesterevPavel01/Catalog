using Calabonga.OperationResults;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Domain.Entities;

namespace Catalog.ModuleCompositionValidator
{
    public class CompositionValidator
    {
        private List<ModuleCompositionRule> _moduleCompositionRules = null!;

        public void SetModuleCompositionRules(List<ModuleCompositionRule> moduleCompositionRules)
        {
            _moduleCompositionRules = moduleCompositionRules;
        }

        public Operation<bool, string> Validate(Module module, Component component)
        { 
            foreach (var numericParameter in module.ModuleNumericParameters)
            {
                var result = CheckNumericProperty(module, numericParameter, component);

                if (!result.Ok)
                    return result;
            }

            foreach (var textParameter in module.ModuleTextParameters)
            {
                var result = CheckTextProperty(module, textParameter, component);

                if (!result.Ok)
                    return result;
            }

            return true;
        }

        private Operation<bool, string> CheckNumericProperty(Module module, ModuleNumericParameter moduleParameter, Component component)
        {
            var parameterRules = _moduleCompositionRules.FirstOrDefault(x => x.ModuleParameter == moduleParameter.ParameterType.Title.Value);

            if (parameterRules is null)
                return true;

            foreach (var parameterRule in parameterRules.Dependencies)
            {

                var validationProperty = component.ComponentNumericParameters
                    .FirstOrDefault(x =>
                        x.ParameterType.Title.Value == parameterRule.Parameter
                        && (parameterRule.TargetComponentTypes is null || parameterRule.TargetComponentTypes.Contains(component.ComponentType.Title.Value)));

                if (validationProperty is null)
                    continue;

                switch (parameterRule.ComparisonRule.Trim())
                {
                    case ">":
                        if (!(validationProperty.Value >= moduleParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {moduleParameter.ParameterType.Title.Value.ToUpper()} компонента {module.ModuleType.Title.Value.ToUpper()} (код: {module.Code})! Значение {moduleParameter.Value} его свойства {moduleParameter.ParameterType.Title.Value.ToUpper()} должно быть <= значения {validationProperty.Value} свойства {parameterRule.Parameter.ToUpper()} у компонента {component.ComponentType.Title.Value.ToUpper()} (код: {component.Code})");
                        break;

                    case "<":
                        if (!(validationProperty.Value <= (moduleParameter.Value)))
                            return Operation.Error($"Не выполняется условие для свойства {moduleParameter.ParameterType.Title.Value.ToUpper()} компонента {module.ModuleType.Title.Value.ToUpper()} (код: {module.Code})! Значение {moduleParameter.Value} его свойства {moduleParameter.ParameterType.Title.Value.ToUpper()} должно быть >= значения {validationProperty.Value} свойства {parameterRule.Parameter.ToUpper()} у компонента {component.ComponentType.Title.Value.ToUpper()} (код: {component.Code})");
                        break;
                    case "=":
                        if (validationProperty.Value != (moduleParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {moduleParameter.ParameterType.Title.Value.ToUpper()} компонента {module.ModuleType.Title.Value.ToUpper()} (код: {module.Code})! Значение {moduleParameter.Value} его свойства {moduleParameter.ParameterType.Title.Value.ToUpper()} должно быть равно значению {validationProperty.Value} свойства {parameterRule.Parameter.ToUpper()} у компонента {component.ComponentType.Title.Value.ToUpper()} (код: {component.Code})");
                        break;
                    case "!=":
                        if (validationProperty.Value == (moduleParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {moduleParameter.ParameterType.Title.Value.ToUpper()} компонента {module.ModuleType.Title.Value.ToUpper()} (код: {module.Code})! Значение {moduleParameter.Value} его свойства {moduleParameter.ParameterType.Title.Value.ToUpper()} должно быть не равно значению {validationProperty.Value} свойства {parameterRule.Parameter.ToUpper()} у компонента {component.ComponentType.Title.Value.ToUpper()} (код: {component.Code})");
                        break;
                    default:
                        return Operation.Error($"ComparisonRule not found {parameterRule.ComparisonRule} (ComponentProperty: {moduleParameter.ParameterType.Title.Value.ToUpper()}. Type parameter: {moduleParameter.GetType().Name})");
                }
            }
            return true;
        }

        private Operation<bool, string> CheckTextProperty(Module module, ModuleTextParameter moduleParameter, Component component)
        {
            var parameterRules = _moduleCompositionRules.FirstOrDefault(x => x.ModuleParameter == moduleParameter.ParameterType.Title.Value);

            if (parameterRules is null)
                return true;

            foreach (var parameterRule in parameterRules.Dependencies)
            {
                var componentValidationParameters = component.ComponentTextParameters
                    .Where(x =>
                        x.ParameterType.Title.Value == parameterRule.Parameter
                        && (parameterRule.TargetComponentTypes is null || parameterRule.TargetComponentTypes.Contains(component.ComponentType.Title.Value))).ToList();

                if (componentValidationParameters is null || componentValidationParameters.Count == 0)
                    continue;

                switch (parameterRule.ComparisonRule)
                {
                    case "Contains":
                        if (!componentValidationParameters.Select(x => x.Value).Contains(moduleParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {moduleParameter.ParameterType.Title.Value} модуля: {module.ModuleType.Title.Value} (код: {module.Code})! Значение {moduleParameter.Value.Value} его свойства {moduleParameter.ParameterType.Title.Value} не найдено в списке разрешенных значений({componentValidationParameters.Select(x => x.Value)}) у компонента {component.ComponentType.Title} (код: {component.Code}) ");
                        break;
                    case "NoContains":
                        if (componentValidationParameters.Select(x => x.Value).Contains(moduleParameter.Value))
                            return Operation.Error($"Не выполняется условие для свойства {moduleParameter.ParameterType.Title.Value} модуля: {module.ModuleType.Title.Value} (код: {module.Code})! Значение {moduleParameter.Value.Value} его свойства {moduleParameter.ParameterType.Title.Value} найдено в списке значений({componentValidationParameters.Select(x => x.Value)}) у компонента {component.ComponentType.Title} (код: {component.Code}) ");
                        break;
                    default:
                        return Operation.Error($"ComparisonRule not found {parameterRule.ComparisonRule} (ComponentProperty: {moduleParameter.ParameterType.Title.Value}. Type property: {moduleParameter.GetType().Name})");
                }
            }
            return true;
        }
    }
}