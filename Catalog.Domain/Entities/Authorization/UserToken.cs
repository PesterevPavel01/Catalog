using Catalog.Domain.Entities.Base;

namespace Catalog.Domain.Entities.Authorization
{
    public class UserToken : Entity
    {
        public UserToken(Guid id) : base(id)
        {
        }

        public string RefreshToken { get; set; } = null!;
        public DateTime RefreshTokenExpiryTime { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}
