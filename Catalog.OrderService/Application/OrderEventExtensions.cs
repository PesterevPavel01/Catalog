using Catalog.Contracts.Enum;

namespace Catalog.OrderService.Application
{
    public static class OrderEventTypesExtensions
    {
        public static bool IsCaching(this OrderEventType eventTypes)
        {
            return eventTypes switch
            {
                OrderEventType.Created => true,
                OrderEventType.Changed => true,
                OrderEventType.Deleted => true,
                OrderEventType.Cancelled => true,
                OrderEventType.Completed => true,
                OrderEventType.CreateApprovalWorkflow => true,
                OrderEventType.ApprovalCompleted => true,
                OrderEventType.Exported => true,
                OrderEventType.Disabled => true,
                OrderEventType.ExternallyRejected => true,
                OrderEventType.Reject =>  true,
                OrderEventType.Produced => true,
                OrderEventType.MessageAdded => true,
                _ => false
            };
        }
    }
}
