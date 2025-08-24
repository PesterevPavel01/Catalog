using Calabonga.OperationResults;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

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
            List<ComponentRequaredRarameter> requaredProperties,
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

            if (requaredProperties is null)
                return Operation.Error("RequaredProperties not found");

            var checkResult = CheckRequaredProperty(componentType, requaredProperties, textParameters, numericParameters);

            if (!checkResult.Ok)
                return Operation.Error(checkResult.Error);

            var component =new Component(titleValue.Result, code, Guid.Empty)
                .SetComponentType(componentType);

            textParameters?.ForEach(x => component.AddTextParameter(x));
            numericParameters?.ForEach(x => component.AddNumericParameter(x));

            return component;

        }

        private static Operation<bool, string> CheckRequaredProperty(
            ComponentType componentType,
            List<ComponentRequaredRarameter> requaredProperties,
            List<ComponentTextParameter>? textParameters = null,
            List<ComponentNumericParameter>? numericParameters = null)
        {
            var requaredField = requaredProperties.FirstOrDefault(x => x.ComponentType == componentType.Title.Value);

            if (requaredField == null)
                return true;

            if (requaredField is null || 
                (requaredField.Fields
                    .FirstOrDefault(x => 
                        (
                            numericParameters is null || !numericParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                            && (textParameters is null || !textParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                        ) is not null))
                return Operation.Error("У модели отсутствуют обязательные поля");

            return true;
        }

        public Component SetComponentType(ComponentType componentType)
        {
            ComponentType = componentType;
            return this;
        }

        public void AddTextParameter(ComponentTextParameter textParameter) 
        {
            var exists = _componentTextParameters.Find(x => x.Id == textParameter.Id);
            if (exists is not null) 
                return;

            _componentTextParameters.Add(textParameter);
        }

        public void AddNumericParameter(ComponentNumericParameter numericParameter)
        {
            var exists = _componentNumericParameters.Find(x => x.Id == numericParameter.Id);
            if (exists is not null)
                return;

            _componentNumericParameters.Add(numericParameter);
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
    }
}
