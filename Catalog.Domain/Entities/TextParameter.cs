using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.Enum;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public class TextParameter : Entity
    {
        private TextParameter(TextParameterValue value, Guid id) : base(id)
        {
            Value = value;
        }
        
        public static Operation<TextParameter, string>  Create(string value, ParameterType parameterType)
        {
            if (parameterType is null)
                return Operation.Error("ParameterType not found");

            if (parameterType.Type != ParameterValueType.Text)
                return Operation.Error("TextParameter cannot have a non-text parameter type");

            var textParameterValue = TextParameterValue.Create(value);
            
            if (!textParameterValue.Ok)
                throw new Exception(textParameterValue.Error);

            return new TextParameter(textParameterValue.Result, Guid.Empty).SetType(parameterType);
        }
        
        public Guid ParameterTypeId { get; private set; }
        public ParameterType ParameterType { get; private set; } = null!;

        public Guid ComponentId { get; private set; }
        public TextParameterValue Value { get; private set; }

        public TextParameter SetType(ParameterType parameterType)
        {
            ParameterType = parameterType;
            return this;
        }
    }
}
