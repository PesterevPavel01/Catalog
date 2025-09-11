using Calabonga.OperationResults;
using Catalog.Domain.Entities;

namespace Catalog.Contracts.Interfaces
{
    public interface IComponentParametersValidator
    {
        Operation<bool, string> Validate(Component component);
    }
}
