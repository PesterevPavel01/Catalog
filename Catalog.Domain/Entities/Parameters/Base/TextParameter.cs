using Calabonga.OperationResults;
using Catalog.Contracts.Dto;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.Enum;
using Catalog.Domain.ValueObjects;

namespace Catalog.Contracts.Entities.Parameters.Base
{
    public class TextParameter : Entity
    {
        protected TextParameter(TextParameterValue value, Guid id) : base(id)
        {
            Value = value;
        }

        public TextParameterValue Value { get; private set; }

        public Guid ParameterTypeId { get; private set; }
        public ParameterType ParameterType { get; private set; } = null!;

        public Operation<bool, string> SetType(ParameterType parameterType)
        {
            if (parameterType is null)
                return Operation.Error("ParameterType not found");

            if (parameterType.Type != ParameterValueType.Text)
                return Operation.Error("TextParameter cannot have a non-text parameter type");

            ParameterType = parameterType;

            return true;
        }

        public TextParameterDto ConvertToDto()
            => new TextParameterDto()
            {
                Type = ParameterType.Title.Value,
                TypeCode = ParameterType.Code,
                Value = Value.Value
            };
    }
}
