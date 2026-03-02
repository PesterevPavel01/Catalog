using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Order;
using Catalog.Domain.Entities;

namespace Catalog.Contracts.Interfaces;

public interface IOrderValidator
{
    public Operation<bool, string> Validate(Order order);
    public Operation<bool, string> Validate(OrderDto orderDto);
}
