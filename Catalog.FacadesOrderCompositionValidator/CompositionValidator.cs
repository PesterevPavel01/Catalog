using Calabonga.OperationResults;
using Catalog.Contracts.Configurations;
using Catalog.Contracts.Dto.Order;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Catalog.FacadesOrderCompositionValidator
{
    public class CompositionValidator : IOrderValidator
    {
        private readonly OrderConfiguration _orderCompositionRules;

        public CompositionValidator(IOptions<OrderConfiguration> options)
        {
            _orderCompositionRules = options.Value;
        }

        public Operation<bool, string> Validate(Order order) {

            foreach (var rule in _orderCompositionRules.UniformOrderParameters)
            {
                var existingTextParameters = order.OrderItems
                    .Select(x => x.Module)
                    .SelectMany(m => m.Components.Where(c => rule.TargetComponentTypes is null || rule.TargetComponentTypes.Contains(c.ComponentType.Title.Value)))
                    .SelectMany(c => c.ComponentTextParameters)
                    .Where(x => x.ParameterType.Title.Value == rule.Parameter);

                var existingNumericParameters = order.OrderItems
                    .Select(x => x.Module)
                    .SelectMany(m => m.Components.Where(c => rule.TargetComponentTypes is null || rule.TargetComponentTypes.Contains(c.ComponentType.Title.Value)))
                    .SelectMany(c => c.ComponentNumericParameters)
                    .Where(x => x.ParameterType.Title.Value == rule.Parameter).ToArray();

                if (existingTextParameters.Any() && existingTextParameters.GroupBy(x => x.Value).Count() > 1)
                    return Operation.Error($"В заказ не может быть добавлено несколько фасадов с разными значениями параметра {rule.Parameter} для компонентов {JsonConvert.SerializeObject(rule.TargetComponentTypes, Formatting.Indented)})");
                    

                if (existingNumericParameters.Any() && existingNumericParameters.GroupBy(x => x.Value).Count() > 1)
                    return Operation.Error($"В заказ не может быть добавлено несколько фасадов с разными значениями параметра {rule.Parameter} для компонентов {JsonConvert.SerializeObject(rule.TargetComponentTypes, Formatting.Indented)}");

            }

            return true;
        }

        public Operation<bool, string> Validate(OrderDto orderDto)
        {
            foreach (var rule in _orderCompositionRules.UniformOrderParameters)
            {
                var existingTextParameters = orderDto.Modules
                    .Select(x => x.Module)
                    .SelectMany(m => m.Components.Where(c => rule.TargetComponentTypes is null || rule.TargetComponentTypes.Contains(c.ComponentTypeTitle)))
                    .SelectMany(c => c.TextParameters)
                    .Where(x => x.Type == rule.Parameter);

                var existingNumericParameters = orderDto.Modules
                    .Select(x => x.Module)
                    .SelectMany(m => m.Components.Where(c => rule.TargetComponentTypes is null || rule.TargetComponentTypes.Contains(c.ComponentTypeTitle)))
                    .SelectMany(c => c.NumericParameters)
                    .Where(x => x.Type == rule.Parameter);

                if (existingTextParameters.Any() && existingTextParameters.GroupBy(x => x.Value).Count() > 1)
                    return Operation.Error($"В заказ не может быть добавлено несколько фасадов с разными значениями параметра {rule.Parameter} для компонентов {JsonConvert.SerializeObject(rule.TargetComponentTypes, Formatting.Indented)}");


                if (existingNumericParameters.Any() && existingNumericParameters.GroupBy(x => x.Value).Count() > 1)
                    return Operation.Error($"В заказ не может быть добавлено несколько фасадов с разными значениями параметра {rule.Parameter} для компонентов {JsonConvert.SerializeObject(rule.TargetComponentTypes, Formatting.Indented)}");

            }

            return true;
        }

    }
}