using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.Enum;
using Catalog.Domain.ValueObjects;

namespace Catalog.Contracts.Entities.Parameters.Base
{
    public class ParameterType : SimpleEntity
    {
        private readonly List<ComponentTextParameter> _componentTextParameters = [];
        private readonly List<ModuleTextParameter> _moduleTextParameters = [];
        private readonly List<ComponentNumericParameter> _componentNumericParameters = [];
        private readonly List<ModuleNumericParameter> _moduleNumericParameters = [];

        private ParameterType(TitleValue title, string code, Guid id, ParameterValueType type) : base(title, code, id)
        {   
            Type = type;
        }

        public static Operation<ParameterType, string> Create(string title, string code, ParameterValueType type)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Operation.Error("Value is empty or null");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Operation.Error("Code is empty or null");
            }

            var titleValue = TitleValue.Create(title);

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            return new ParameterType(titleValue.Result, code, Guid.Empty, type);
        }

        public ParameterValueType Type {  get; private set; }
        public IReadOnlyCollection<ComponentTextParameter> ComponentTextParameters => _componentTextParameters.AsReadOnly();
        public IReadOnlyCollection<ModuleTextParameter> ModuleTextParameters => _moduleTextParameters.AsReadOnly();
        public IReadOnlyCollection<ComponentNumericParameter> ComponentNumericParameters => _componentNumericParameters.AsReadOnly();
        public IReadOnlyCollection<ModuleNumericParameter> ModuleNumericParameters => _moduleNumericParameters.AsReadOnly();

    }
}
