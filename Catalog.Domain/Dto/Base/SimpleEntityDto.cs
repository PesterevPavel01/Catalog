namespace Catalog.Contracts.Dto.Base
{
    public record SimpleEntityDto
    {
        public required string Title { get; set; }
        public required string Code { get; set; }
    }
}
