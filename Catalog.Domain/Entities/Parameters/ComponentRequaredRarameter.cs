namespace Catalog.Contracts.Entities.Parameters
{
    public record ComponentRequaredRarameter 
    {
        public string ComponentType { get; set; }
        public List<string> Fields { get; set; }
    }
}
