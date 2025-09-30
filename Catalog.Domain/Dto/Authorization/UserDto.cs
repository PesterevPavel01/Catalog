namespace Catalog.Contracts.Dto.Authorization
{
    public sealed record UserDto
    {
        public required string UserName { get; set; }

        public required string ExternalId { get; set; }

        public IEnumerable<string> Roles { get; set; } = [];
    }
}
