using Calabonga.OperationResults;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Contracts.Entities.Parameters
{
    public class ComponentTextParameter : TextParameter
    {
        public ComponentTextParameter(TextParameterValue value, Guid id) : base(value, id){}

        public static Operation<ComponentTextParameter, string> Create(string value, ParameterType parameterType)
        {
            var textParameterValue = TextParameterValue.Create(value);

            if (!textParameterValue.Ok)
                throw new Exception(textParameterValue.Error);

            var conponentTextParameter = new ComponentTextParameter(textParameterValue.Result, Guid.Empty);

            var setTypeResult = conponentTextParameter.SetType(parameterType);

            if (!setTypeResult.Ok)
                return Operation.Error(setTypeResult.Error);

            return conponentTextParameter;
        }

        public Guid ComponentId { get; private set; }
    }
}
