namespace Catalog.Domain.Dto.Authorization
{
    public sealed record LoginDto
    {
        public required string UserName { get; set; }

        public required string Password { get; set; }
    }
}
