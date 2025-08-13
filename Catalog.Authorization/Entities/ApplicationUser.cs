namespace Catalog.Domain.Entities.Autorization
{
    public class ApplicationUser : Entity
    {
        public ApplicationUser(Guid id) : base(id)
        {
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public UserToken UserToken { get; set; } = null!;
    }
}
