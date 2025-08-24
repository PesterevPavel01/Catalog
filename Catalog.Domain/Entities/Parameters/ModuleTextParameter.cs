using Calabonga.OperationResults;
using Catalog.Contracts.Entities.Parameters.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Contracts.Entities.Parameters
{
    public sealed class ModuleTextParameter : TextParameter
    {
        public ModuleTextParameter(TextParameterValue value, Guid id) : base(value, id){}

        public static Operation<ModuleTextParameter, string> Create(string value, ParameterType parameterType)
        {
            var textParameterValue = TextParameterValue.Create(value);

            if (!textParameterValue.Ok)
                throw new Exception(textParameterValue.Error);

            var moduleTextParameter = new ModuleTextParameter(textParameterValue.Result, Guid.Empty);

            var setTypeResult = moduleTextParameter.SetType(parameterType);

            if (!setTypeResult.Ok)
                return Operation.Error(setTypeResult.Error);

            return moduleTextParameter;
        }

        public Guid ModuleId { get; private set; }
    }
}
