using Calabonga.OperationResults;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Catalog.ModuleParametersValidator
{
    public class ModuleParametersValidator: IModuleParametersValidator
    {
        private readonly IEnumerable<ModuleTechnologicalRestriction> _technologicalRestrictions;
        private readonly IEnumerable<ModuleRequiredParameter> _requiredParameters;
        private readonly bool _allowModifyModuleWithCompletedOrders;

        public ModuleParametersValidator(IOptions<ModuleConfiguration> options)
        {
            _technologicalRestrictions = [.. options.Value.ModuleTechnologicalRestrictions];
            _requiredParameters = [.. options.Value.ModuleRequiredParameters];
            _allowModifyModuleWithCompletedOrders = options.Value.AllowModifyModuleWithCompletedOrders;
        }

        public Operation<bool, string> Validate(Module module, Component? component = null)
        {
            if (_requiredParameters is null)
                return Operation.Error("RequiredParameters not found");

            if (!_allowModifyModuleWithCompletedOrders && module.OrderItems.Any() && module.OrderItems.FirstOrDefault(item => item.ApprovalWorkflow is null || item.ApprovalWorkflow.IsCompleted == false) is null)
            {
                return Operation.Error("Модуль не может быть изменен т.к. у модуля завершен процесс согласования.");
                //return Operation.Error("The module cannot be modified because it has completed orders.");
            }

            foreach (var parameter in module.ModuleNumericParameters)
            {
                var operationResult = CheckTechnologicalRestrictions(parameter, module.ModuleType.Code);

                if(!operationResult.Ok)
                    return operationResult;
            }

            var checkRequiredParametersResult = CheckRequiredParameters(module, module.ModuleType.Code);

            if (!checkRequiredParametersResult.Ok)
                return checkRequiredParametersResult;

            return true;
        }

        private Operation<bool, string> CheckRequiredParameters(Module module, string moduleTypeCode, Component? component = null)
        {
            var moduleRequiredParameter = _requiredParameters.FirstOrDefault(x => x.ModuleTypeCode == moduleTypeCode);

            if (moduleRequiredParameter == null)
                return Operation.Error("ModuleRequiredParameters not found");

            //проверяем наличие параметров, которые должны быть при создании
            var requiredParameters = moduleRequiredParameter.Parameters.Where(x => x.Dependencies is null).ToList();

            if (requiredParameters.Count == 0)
                return Operation.Error("Default ModuleRequiredParameters not found");

            if (requiredParameters
                .Select(x => x.Parameter)
                .FirstOrDefault(x =>
                (
                    module.ModuleNumericParameters is null || !module.ModuleNumericParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                    && (module.ModuleTextParameters is null || !module.ModuleTextParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                ) is not null)
                return Operation.Error("Required parameters are missing from the module");

            if (component is null)
                return true;

            //проверяем параметры, которые необходимы для компонента
            requiredParameters = [..moduleRequiredParameter.Parameters
                .Where(x => x.Dependencies is not null).ToList()];

            requiredParameters = [..requiredParameters
                .Where(x => x.Dependencies
                    .Select(x => x.ComponentsTypeTitle).Contains(component.ComponentType.Title.Value))];

            if (requiredParameters.Count == 0)
                return true;

            requiredParameters = [.. requiredParameters
                .Where(x => x.Dependencies
                    .Select(x => x.ComponentsTitle).Contains(component.Title.Value))];

            if (requiredParameters.Count == 0)
                return true;

            if (requiredParameters
                .Select(x => x.Parameter)
                .FirstOrDefault(x =>
                (
                    module.ModuleNumericParameters is null || !module.ModuleNumericParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                    && (module.ModuleTextParameters is null || !module.ModuleTextParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                ) is not null)
                return Operation.Error("Required parameters are missing from the module");

            return true;
        }

        private Operation<bool, string> CheckTechnologicalRestrictions(ModuleNumericParameter targetParameter, String moduleTypeCode)
        {
            var technologicalRestrictions = _technologicalRestrictions.Where(x => x.ModuleTypeCode == moduleTypeCode).ToList();

            var parameterRules = technologicalRestrictions.Where(x => x.Parameter == targetParameter.ParameterType.Title.Value);

            if (!parameterRules.Any())
                return true;

            foreach (var parameterRule in parameterRules)
            {
                switch (parameterRule.ComparisonRule.Trim())
                {
                    case "<":
                        if (!(targetParameter.Value <= parameterRule.Value))
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value.ToUpper()}! Значение {targetParameter.Value} свойства {targetParameter.ParameterType.Title.Value.ToUpper()} должно быть <= значения {parameterRule.Value}");
                        break;

                    case ">":
                        if (!(targetParameter.Value >= parameterRule.Value))
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value.ToUpper()}! Значение {targetParameter.Value} свойства {targetParameter.ParameterType.Title.Value.ToUpper()} должно быть >= значения {parameterRule.Value}");
                        break;
                    case "=":
                        if (targetParameter.Value != parameterRule.Value)
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value.ToUpper()}! Значение {targetParameter.Value} свойства {targetParameter.ParameterType.Title.Value.ToUpper()} должно быть != значению {parameterRule.Value}");
                        break;
                    case "!=":
                        if (targetParameter.Value == parameterRule.Value)
                            return Operation.Error($"Не выполняется условие для свойства {targetParameter.ParameterType.Title.Value.ToUpper()}! Значение {targetParameter.Value} свойства {targetParameter.ParameterType.Title.Value.ToUpper()} должно быть = значению {parameterRule.Value}");
                        break;
                    default:
                        return Operation.Error($"ComparisonRule not found {parameterRule.ComparisonRule} (ComponentParameter: {targetParameter.ParameterType.Title.Value.ToUpper()}. Type parameter: {targetParameter.GetType().Name})");
                }
            }
            return true;
        }
    }
}
