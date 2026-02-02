using Catalog.Contracts.Enum;

namespace Catalog.OrderService.Application
{
    public static class OrderEventTypesExtensions
    {
        public static bool IsCaching(this OrderEventTypes eventTypes)
        {
            return eventTypes switch
            {
                OrderEventTypes.Created => true,
                OrderEventTypes.Changed => true,
                OrderEventTypes.Deleted => true,
                OrderEventTypes.Cancelled => true,
                OrderEventTypes.Completed => true,
                OrderEventTypes.CreateApprovalWorkflow => true,
                OrderEventTypes.ApprovalCompleted => true,
                OrderEventTypes.Exported => true,
                OrderEventTypes.Disabled => true,
                OrderEventTypes.ExternallyRejected => true,
                OrderEventTypes.Reject =>  true,
                OrderEventTypes.Produced => true,
                OrderEventTypes.MessageAdded => true,
                _ => false
            };
        }
    }
}
