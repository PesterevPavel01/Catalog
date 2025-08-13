using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.Enum;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public class ParameterType : SimpleEntity
    {
        private readonly List<TextParameter> _textParameters = [];
        private readonly List<NumericParameter> _numericParameters = [];

        private ParameterType(TitleValue title, CodeValue code, Guid id, ParameterValueType type) : base(title, code, id)
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

            var codeValue = CodeValue.Create(code);

            if (!codeValue.Ok)
                return Operation.Error(codeValue.Error);

            return new ParameterType(titleValue.Result, codeValue.Result, Guid.Empty, type);
        }

        public ParameterValueType Type {  get; private set; }

        public IReadOnlyCollection<TextParameter> TextParameters => _textParameters.AsReadOnly();

        public IReadOnlyCollection<NumericParameter> NumericParameters => _numericParameters.AsReadOnly();

    }
}
