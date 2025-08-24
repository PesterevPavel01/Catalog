using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.Enum;

namespace Catalog.Contracts.Entities.Parameters.Base
{
    public class NumericParameter: Entity
    {
        protected NumericParameter(double value, Guid id) : base(id)
        {
            Value = value;
        }

        public double Value { get; private set; }

        public Guid ParameterTypeId { get; private set; }
        public ParameterType ParameterType { get; private set; } = null!;

        public Operation<bool, string> SetType(ParameterType parameterType)
        {
            if (parameterType is null)
                return Operation.Error("ParameterType not found");

            if (parameterType.Type != ParameterValueType.Numeric)
                return Operation.Error("NumericParameter cannot have a non-numeric parameter type");

            ParameterType = parameterType;

            return true;
        }
    }
}
