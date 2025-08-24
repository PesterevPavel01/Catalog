using Calabonga.OperationResults;
using Catalog.Contracts.Entities.Parameters.Base;

namespace Catalog.Contracts.Entities.Parameters
{
    public class ComponentNumericParameter : NumericParameter
    {
        public ComponentNumericParameter(double value, Guid id) : base(value, id){}

        public static Operation<ComponentNumericParameter, string> Create(double value, ParameterType parameterType)
        {
            var componentNumericParameter = new ComponentNumericParameter(value, Guid.Empty);

            var setTypeResult = componentNumericParameter.SetType(parameterType);

            if(!setTypeResult.Ok)
                return Operation.Error(setTypeResult.Error);

            return componentNumericParameter;
        }


        public Guid ComponentId { get; private set; }
    }
}
