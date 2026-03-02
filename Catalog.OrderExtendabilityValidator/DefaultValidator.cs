using Calabonga.OperationResults;
using Catalog.Contracts;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities;

namespace Catalog.OrderExtendabilityValidator
{
    public sealed class DefaultValidator : IOrderExtendabilityValidator
    {
        public Operation<bool, string> Validate(Order order, CancellationToken cancellationToken = default)
        {
            if (order.IsApprovalCompleted())
                return Operation.Error("Order is completed!");

            if (order.OrderItems.Where(x => x.ApprovalWorkflow is not null).Any())
                return Operation.Error("У заказа запущен процесс согласования!");

            return true;
        }
    }
}
