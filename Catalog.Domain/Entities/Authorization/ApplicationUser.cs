using Catalog.Domain.Dto;
using System.Text;
using Catalog.Domain.Entities.Base;
using System.Security.Cryptography;

namespace Catalog.Domain.Entities.Autorization
{
    public class ApplicationUser : Entity
    {
        private ApplicationUser(Guid id, string userName, string password, string? email) : base(id)
        {
            UserName = userName;
            Password = password;
            Email = email;
        }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public string? Email { get; private set; }

        public UserToken UserToken { get; set; } = null!;

        public static ApplicationUser Create(Guid id, string userName, string password, string? email = null)
            => new(id, userName, HashPassword(password), email);

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes).ToLower();
        }

        public bool CheckPassword(string password)
            => Password == HashPassword(password);
    }
}
