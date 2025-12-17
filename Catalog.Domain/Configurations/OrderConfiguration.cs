namespace Catalog.Contracts.Configurations
{
    public sealed record OrderConfiguration
    {
        public required Int32 ArchiveStorageDays { get; set; }
        public required List<UniformOrderParameter> UniformOrderParameters { get; set; }
    }
}
