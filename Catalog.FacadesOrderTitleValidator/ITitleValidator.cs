using Calabonga.OperationResults;

namespace Catalog.FacadeOrderTitleValidator
{
    public interface ITitleValidator
    {
        Task<Operation<string, string>> Validate(CancellationToken cancellationToken);
    }
}