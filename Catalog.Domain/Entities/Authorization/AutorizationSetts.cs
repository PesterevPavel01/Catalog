namespace Catalog.Domain.Entities.Authorization
{
    public sealed class AuthorizationSettings
    {
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string Authority { get; set; } = null!;
        public int LifetimeInMinutes { get; set; }
        public int RefreshTokenValidityInDays { get; set; }
        public string AdministratorPassword { get; set; } = null!;
    }
}
