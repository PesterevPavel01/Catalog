using Calabonga.OperationResults;
using Catalog.Domain.Entities;

namespace Catalog.Contracts.Interfaces;

public interface IOrderExtendabilityValidator
{
    Operation<bool, string> Validate(Order order, CancellationToken cancellationToken = default);
}
