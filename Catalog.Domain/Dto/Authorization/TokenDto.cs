namespace Catalog.Domain.Dto.Authorization
{
    public class TokenDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
