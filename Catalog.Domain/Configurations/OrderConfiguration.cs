namespace Catalog.Contracts.Configurations
{
    public sealed record OrderConfiguration
    {
        public required OrderCleanupSettings OrderCleanupSettings { get; set; }
        public required List<UniformOrderParameter> UniformOrderParameters { get; set; }
        public string? CompletionTriggerEventType { get; set; }
    }

    public sealed record OrderCleanupSettings(Int16 CompletedOrdersDays, Int16 InactiveOrdersDays, Int16 DisabledOrdersDays);
}
