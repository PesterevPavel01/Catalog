using Calabonga.OperationResults;
using Catalog.Contracts.Dto;
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
                return Operation.Error(textParameterValue.Error);

            var componentTextParameter = new ComponentTextParameter(textParameterValue.Result, Guid.Empty);

            var setTypeResult = componentTextParameter.SetType(parameterType);

            if (!setTypeResult.Ok)
                return Operation.Error(setTypeResult.Error);

            return componentTextParameter;
        }

        public Guid ComponentId { get; private set; }
    }
}
