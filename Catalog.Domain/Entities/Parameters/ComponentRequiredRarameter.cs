namespace Catalog.Contracts.Entities.Parameters
{
    public record ComponentRequiredRarameter 
    {
        public required string ComponentType { get; set; }
        public string? ComponentTitle { get; set; }
        public required List<string> Parameters { get; set; }
    }
}
