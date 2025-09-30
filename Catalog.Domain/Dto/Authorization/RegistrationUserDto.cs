namespace Catalog.Contracts.Dto.Authorization
{
    public sealed record RegistrationUserDto
    {
        public required string UserName { get; set; }

        public required string Password { get; set; }

        public required string Role { get; set; }
    }
}
