namespace Catalog.Contracts.Enum
{
    public enum OrderEventType
    {
        ApprovalCompleted = 0,
        Cancelled = 1,
        Changed = 2,
        CreateApprovalWorkflow = 3,
        Created = 4,
        CustomModuleModified = 5,
        Disabled = 6,
        Deleted = 7,
        MessageAdded = 8,
        Reject = 9,
        InProduction = 10,
        ExternallyRejected = 11,
        Produced = 12,
        Completed = 13,
        OrderItemQuantityChanged = 14,
        OrderItemRemoved = 14,
    }
}
