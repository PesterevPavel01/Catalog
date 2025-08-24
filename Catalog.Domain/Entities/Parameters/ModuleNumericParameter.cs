using Calabonga.OperationResults;
using Catalog.Contracts.Entities.Parameters.Base;

namespace Catalog.Contracts.Entities.Parameters
{
    public sealed class ModuleNumericParameter : NumericParameter
    {
        public ModuleNumericParameter(double value, Guid id) : base(value, id){}

        public static Operation<ModuleNumericParameter, string> Create(double value, ParameterType parameterType)
        {
            var moduleNumericParameter = new ModuleNumericParameter(value, Guid.Empty);

            var setTypeResult = moduleNumericParameter.SetType(parameterType);

            if (!setTypeResult.Ok)
                return Operation.Error(setTypeResult.Error);

            return moduleNumericParameter;
        }

        public Guid ModuleId { get; private set; }
    }
}
