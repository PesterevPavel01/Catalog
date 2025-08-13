using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.Enum;

namespace Catalog.Domain.Entities
{
    public class NumericParameter : Entity
    {
        private NumericParameter(Double value, Guid id) : base(id)
        {
            Value = value;
        }

        public static Operation<NumericParameter, string>  Create(Double size, ParameterType parameterType)
        {
            if (parameterType is null)
                return Operation.Error("ParameterType not found");

            if (parameterType.Type != ParameterValueType.Numeric)
                return Operation.Error("NumericParameter cannot have a non-numeric parameter type");

            return new NumericParameter(size, Guid.Empty).SetType(parameterType);
        }

        public Guid ParameterTypeId { get; private set; }
        public ParameterType ParameterType { get; private set; } = null!;

        public Guid ComponentId { get; private set; }
        public Double Value { get; private set; }

        public NumericParameter SetType(ParameterType parameterType)
        {
            ParameterType = parameterType;
            return this;
        }
    }
}
