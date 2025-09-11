using Calabonga.OperationResults;
using Catalog.Domain.Entities;

namespace Catalog.Contracts.Interfaces
{
    public interface IModuleParametersValidator
    {
        Operation<bool, string> Validate(Module module, Component? component = null);
    }
}
