using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Components;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Catalog.Domain.Entities
{
    public class Component : SimpleEntity
    {
        private readonly List<ComponentTextParameter> _componentTextParameters = [];
        private readonly List<ComponentNumericParameter> _componentNumericParameters = [];
        private readonly List<Module> _modules = [];

        protected Component(TitleValue title, string code, Guid id) : base(title, code, id)
        {
        }

        public Guid ComponentTypeId { get; private set; }
        public ComponentType ComponentType { get; private set; } = null!;

        public IReadOnlyCollection<ComponentTextParameter> ComponentTextParameters => _componentTextParameters.AsReadOnly();
        public IReadOnlyCollection<ComponentNumericParameter> ComponentNumericParameters => _componentNumericParameters.AsReadOnly();
        public IReadOnlyCollection<Module> Modules => _modules.AsReadOnly();

        public static Operation<Component, string> Create(
            string title, 
            string code, 
            ComponentType componentType,
            List<String> componentMultiplyParameters,
            List<ComponentRequaredRarameter> requaredParameters,
            List<ComponentRequaredRarameter> customComponentRequaredParameters,
            List<ComponentTextParameter>? textParameters = null,
            List<ComponentNumericParameter>? numericParameters = null) 
        {
            if (string.IsNullOrWhiteSpace(title))
                return Operation.Error("Value is empty or null");

            if (string.IsNullOrWhiteSpace(code))
                return Operation.Error("Code is empty or null");

            var titleValue = TitleValue.Create(title);
            
            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            if(componentType is null )
                return Operation.Error("ComponentTupe not found");

            if (requaredParameters is null)
                return Operation.Error("RequaredProperties not found");


            var component =new Component(titleValue.Result, code, Guid.Empty)
                .SetComponentType(componentType);

            if (numericParameters is not null)
            {
                var result = component
                    .AddNumericParameters(
                        numericParameters: numericParameters,
                        componentMultipleParameters: componentMultiplyParameters,
                        componentRequaredParameters: requaredParameters,
                        customComponentRequaredParameters: customComponentRequaredParameters);

                if (!result.Ok)
                    return Operation.Error(result.Error);
            }

            if (textParameters is not null)
            {
                var result = component
                    .AddTextParameters(
                        textParameters: textParameters,
                        componentMultipleParameters: componentMultiplyParameters,
                        componentRequaredParameters: requaredParameters,
                        customComponentRequaredParameters: customComponentRequaredParameters);

                if (!result.Ok)
                        return Operation.Error(result.Error);
            }

            return component;
        }

        public bool IsCostom => CheckCostomization();

        private Operation<bool, string> CheckRequaredParameters(
            List<ComponentRequaredRarameter> componentRequaredParameters,
            List<ComponentRequaredRarameter>  customComponentRequaredParameters)
        {
            var checkResult = CheckPaeameters(
                    requaredParameters: componentRequaredParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            if (!IsCostom)
                return true;

            checkResult = CheckPaeameters(
                requaredParameters: customComponentRequaredParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            return true;
        }

        private Operation<bool, string> CheckPaeameters(
            List<ComponentRequaredRarameter> requaredParameters)
        {
            var currentRequaredParameters = requaredParameters.FirstOrDefault(x => x.ComponentType == ComponentType.Title.Value && x.ComponentTitle is null);

            if (currentRequaredParameters is not null)
            {
                if (currentRequaredParameters is null ||
                    (currentRequaredParameters.Parameters
                        .FirstOrDefault(x =>
                            (
                                ComponentNumericParameters is null || !ComponentNumericParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                                && (ComponentTextParameters is null || !ComponentTextParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                            ) is not null))
                    return Operation.Error("У модели отсутствуют обязательные поля");
            }

            return true;
        }

        private Component SetComponentType(ComponentType componentType)
        {
            ComponentType = componentType;
            return this;
        }

        private bool CheckCostomization()
        {
            var customComponents = ComponentTextParameters.FirstOrDefault(parameter => parameter.ParameterType.Code == "0000000CSTM");

            if (customComponents is not null)
                return true;

            return false;
        }

        public Operation<Component, string> AddTextParameters
            (List<ComponentTextParameter> textParameters, 
            List<String> componentMultipleParameters,
            List<ComponentRequaredRarameter> componentRequaredParameters,
            List<ComponentRequaredRarameter> customComponentRequaredParameters) 
        {
            foreach (var parameter in textParameters)
            {
                var exists = _componentTextParameters.Find(x => x.Id == parameter.Id);

                if (exists is not null && parameter.Id != Guid.Empty)
                    return Operation.Error("the component already has this parameter");

                exists = _componentTextParameters.FirstOrDefault(x => x.ParameterType == parameter.ParameterType && x.Value == parameter.Value);

                if (exists is not null)
                    return Operation.Error("the component already has this parameter");

                exists = _componentTextParameters
                    .FirstOrDefault(x =>
                        !componentMultipleParameters.Contains(parameter.ParameterType.Title.Value)
                        && x.ParameterType.Title.Value == parameter.ParameterType.Title.Value);

                if (exists is not null)
                    return Operation.Error("The component already has a parameter of this type! Parameter is not multiple");

                _componentTextParameters.Add(parameter);
            }

            var checkResult = CheckRequaredParameters(componentRequaredParameters, customComponentRequaredParameters);
            
            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            return this;
        }

        public Operation<Component, string> AddNumericParameters
            (List<ComponentNumericParameter> numericParameters, 
            List<String> componentMultipleParameters,
            List<ComponentRequaredRarameter> componentRequaredParameters,
            List<ComponentRequaredRarameter> customComponentRequaredParameters)
        {
            foreach (var parameter in numericParameters)
            {
                var exists = _componentNumericParameters.Find(x => x.Id == parameter.Id);

                if (exists is not null && parameter.Id != Guid.Empty)
                    return Operation.Error("the component already has this parameter");

                exists = _componentNumericParameters.FirstOrDefault(x => x.ParameterType == parameter.ParameterType && x.Value == parameter.Value);

                if (exists is not null)
                    return Operation.Error("the component already has this parameter");

                exists = _componentNumericParameters
                    .FirstOrDefault(x =>
                        !componentMultipleParameters.Contains(parameter.ParameterType.Title.Value)
                        && x.ParameterType.Title.Value == parameter.ParameterType.Title.Value);

                if (exists is not null)
                    return Operation.Error("The component already has a parameter of this type! Parameter is not multiple");

                _componentNumericParameters.Add(parameter);
            }

            var checkResult = CheckRequaredParameters(componentRequaredParameters, customComponentRequaredParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            return this;
        }

        public Operation<Component, string> RemoveTextParameter
            (ComponentTextParameter textParameter,
            List<ComponentRequaredRarameter> componentRequaredParameters,
            List<ComponentRequaredRarameter> customComponentRequaredParameters)
        {
            var exists = _componentTextParameters.Find(x => x.Id == textParameter.Id);
            
            if (exists is null)
                return this;

            _componentTextParameters.Remove(textParameter);

            var checkResult = CheckRequaredParameters(componentRequaredParameters, customComponentRequaredParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            return this;
        }

        public Operation<Component, string> RemoveNumericParameter
            (ComponentNumericParameter numericParameter,
            List<ComponentRequaredRarameter> componentRequaredParameters,
            List<ComponentRequaredRarameter> customComponentRequaredParameters)
        {
            var exists = _componentNumericParameters.Find(x => x.Id == numericParameter.Id);
            if (exists is null)
                return this;

            _componentNumericParameters.Remove(numericParameter);

            var checkResult = CheckRequaredParameters(componentRequaredParameters, customComponentRequaredParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            return this;
        }

        public ComponentDto ConvertToDto() 
            => 
            new ComponentDto()
            {
                ComponentCode = Code,
                ComponentTitle = Title.Value,
                ComponentTypeTitle = ComponentType?.Title.Value,
                ComponentTypeCode = ComponentType?.Code,
                TextParameters = [.. ComponentTextParameters
                                .Select(x => x.ConvertToDto())],
                NumericParameters = [.. ComponentNumericParameters
                                 .Select(x => x.ConvertToDto())]
            };

        public static Func<IQueryable<Component>, IIncludableQueryable<Component, object>> IncludeRequaredField()
            =>
                query => query
                    .Include(x => x.ComponentType)
                    .Include(x => x.ComponentNumericParameters)
                        .ThenInclude(x => x.ParameterType)
                    .Include(x => x.ComponentTextParameters)
                        .ThenInclude(x => x.ParameterType);
    }
}
