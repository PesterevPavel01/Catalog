using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public class Component : SimpleEntity
    {
        private readonly List<TextParameter> _textParameters = [];
        private readonly List<NumericParameter> _numericParameters = [];
        private readonly List<Module> _modules = [];

        protected Component(TitleValue title, CodeValue code, Guid id) : base(title, code, id)
        {
        }

        public static Operation<Component, string> Create(string title, string code, ComponentType componentType) 
        {
            if(componentType is null )
                return Operation.Error("ComponentTupe not found");

            /*if (module is null)
                return Operation.Error("Module not found");*/

            if (string.IsNullOrWhiteSpace(title))
                return Operation.Error("Value is empty or null");

            if (string.IsNullOrWhiteSpace(code))
                return Operation.Error("Code is empty or null");

            var titleValue = TitleValue.Create(title);
            
            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            var codeValue = CodeValue.Create(code);
            
            if (!codeValue.Ok)
                return Operation.Error(codeValue.Error);

            return new Component(titleValue.Result, codeValue.Result, Guid.Empty)
                .SetComponentType(componentType);
                //.SetModule(module);
        }

        public IReadOnlyCollection<TextParameter> TextParameters => _textParameters.AsReadOnly();
        public IReadOnlyCollection<NumericParameter> NumericParameters => _numericParameters.AsReadOnly();
        public IReadOnlyCollection<Module> Modules => _modules.AsReadOnly();

        public Guid ComponentTypeId { get; private set; }
        public ComponentType ComponentType { get; private set; } = null!;
        /*public Guid ModuleId { get; private set; }
        public Module Module { get; private set; } = null!;

        public Component SetModule(Module module)
        {
            Module = module;
            return this;
        }
        */
        public Component SetComponentType(ComponentType componentType)
        {
            ComponentType = componentType;
            return this;
        }

        public void AddTextParameter(TextParameter textParameter) 
        {
            var exists = _textParameters.Find(x => x.Id == textParameter.Id);
            if (exists is not null) 
                return;

            _textParameters.Add(textParameter);
        }

        public void AddNumericParameter(NumericParameter numericParameter)
        {
            var exists = _numericParameters.Find(x => x.Id == numericParameter.Id);
            if (exists is not null)
                return;

            _numericParameters.Add(numericParameter);
        }

        public void RemoveTextParameter(TextParameter textParameter)
        {
            var exists = _textParameters.Find(x => x.Id == textParameter.Id);
            if (exists is null)
                return;

            _textParameters.Remove(textParameter);
        }

        public void RemoveNumericParameter(NumericParameter numericParameter)
        {
            var exists = _numericParameters.Find(x => x.Id == numericParameter.Id);
            if (exists is null)
                return;

            _numericParameters.Remove(numericParameter);
        }
    }
}
