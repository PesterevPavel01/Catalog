using Calabonga.OperationResults;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using Catalog.Contracts.Dto.Components;
using System.Linq;
using Catalog.Contracts.Entities.Parameters.Base;

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

            if (textParameters is not null)
                foreach (var parameter in textParameters)
                {
                    var result = component.AddTextParameter(parameter, componentMultiplyParameters);
                    if (!result.Ok)
                        return Operation.Error(result.Error);
                }

            if (numericParameters is not null)
                foreach (var parameter in numericParameters)
                {
                    var result = component.AddNumericParameter(parameter, componentMultiplyParameters);
                    if (!result.Ok)
                        return Operation.Error(result.Error);
                }

            var checkResult = CheckRequaredPaeameters(component, requaredParameters, textParameters, numericParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            return component;

        }

        private static Operation<bool, string> CheckRequaredPaeameters(
            Component component,
            List<ComponentRequaredRarameter> requaredProperties,
            List<ComponentTextParameter>? textParameters = null,
            List<ComponentNumericParameter>? numericParameters = null)
        {
            var requaredParameters = requaredProperties.FirstOrDefault(x => x.ComponentType == component.ComponentType.Title.Value && x.ComponentTitle is null);

            if (requaredParameters is not null)
            {
                if (requaredParameters is null ||
                    (requaredParameters.Parameters
                        .FirstOrDefault(x =>
                            (
                                numericParameters is null || !numericParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                                && (textParameters is null || !textParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                            ) is not null))
                    return Operation.Error("У модели отсутствуют обязательные поля");
            }

            requaredParameters = requaredProperties.FirstOrDefault(x => x.ComponentType == component.ComponentType.Title.Value && x.ComponentTitle == component.Title.Value);

            if (requaredParameters is not null)
            {
                if (requaredParameters is null ||
                    (requaredParameters.Parameters
                        .FirstOrDefault(x =>
                            (
                                numericParameters is null || !numericParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                                && (textParameters is null || !textParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                            ) is not null))
                    return Operation.Error("У модели отсутствуют обязательные поля");
            }

            return true;
        }

        public Component SetComponentType(ComponentType componentType)
        {
            ComponentType = componentType;
            return this;
        }

        public Operation<Component, string> AddTextParameter(ComponentTextParameter textParameter, List<String> componentMultipleParameters) 
        {
            var exists = _componentTextParameters.Find(x => x.Id == textParameter.Id);

            if (exists is not null && textParameter.Id != Guid.Empty)
                return Operation.Error("the component already has this parameter");

            exists = _componentTextParameters.FirstOrDefault(x => x.ParameterType == textParameter.ParameterType && x.Value == textParameter.Value);

            if (exists is not null)
                return Operation.Error("the component already has this parameter");

            exists = _componentTextParameters
                .FirstOrDefault(x =>
                    !componentMultipleParameters.Contains(textParameter.ParameterType.Title.Value)
                    && x.ParameterType.Title.Value == textParameter.ParameterType.Title.Value);

            if (exists is not null)
                return Operation.Error("The component already has a parameter of this type! Parameter is not multiple");

            _componentTextParameters.Add(textParameter);

            return this;
        }

        public Operation<Component, string> AddNumericParameter(ComponentNumericParameter numericParameter, List<String> componentMultipleParameters)
        {
            var exists = _componentNumericParameters.Find(x => x.Id == numericParameter.Id);

            if (exists is not null && numericParameter.Id != Guid.Empty)
                    return Operation.Error("the component already has this parameter");
            
            exists = _componentNumericParameters.FirstOrDefault(x => x.ParameterType == numericParameter.ParameterType && x.Value == numericParameter.Value);

            if (exists is not null)
                return Operation.Error("the component already has this parameter");

            exists = _componentNumericParameters
                .FirstOrDefault(x =>
                    !componentMultipleParameters.Contains(numericParameter.ParameterType.Title.Value)
                    && x.ParameterType.Title.Value == numericParameter.ParameterType.Title.Value);

            if (exists is not null)
                return Operation.Error("The component already has a parameter of this type! Parameter is not multiple");

            _componentNumericParameters.Add(numericParameter);

            return this;
        }

        public void RemoveTextParameter(ComponentTextParameter textParameter)
        {
            var exists = _componentTextParameters.Find(x => x.Id == textParameter.Id);
            if (exists is null)
                return;

            _componentTextParameters.Remove(textParameter);
        }

        public void RemoveNumericParameter(ComponentNumericParameter numericParameter)
        {
            var exists = _componentNumericParameters.Find(x => x.Id == numericParameter.Id);
            if (exists is null)
                return;

            _componentNumericParameters.Remove(numericParameter);
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
