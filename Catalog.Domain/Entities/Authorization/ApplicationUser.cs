using System.Security.Cryptography;
using System.Text;
using Calabonga.OperationResults;
using Catalog.Contracts.Entities.Approval;
using Catalog.Contracts.Entities.Authorization;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities.Authorization
{
    public class ApplicationUser : Entity
    {
        private readonly List<Order> _orders = [];
        private readonly List<Role> _roles = [];
        private readonly List<ApprovalWorkflowItem> _approvalWorkflowItems = [];

        private ApplicationUser(Guid id, string userName, PasswordValue password, string? email) : base(id)
        {
            UserName = userName;
            Password = password;
            Email = email;
        }

        public string UserName { get; private set; }
        public PasswordValue Password { get; private set; }
        public string? Email { get; private set; }
        public UserToken UserToken { get; set; } = null!;

        public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();
        public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
        public IReadOnlyCollection<ApprovalWorkflowItem> ApprovalWorkflowItems => _approvalWorkflowItems.AsReadOnly();

        public static Operation<ApplicationUser, string> Create(Guid id, string userName, string? password = "DEFAULT_PASSWORD", string? email = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Operation.Error("UserName is empty or null");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return Operation.Error("Password is empty or null");
            }

            var passwordValue = PasswordValue.Create(HashPassword(password));

            if (!passwordValue.Ok)
                return Operation.Error(passwordValue.Error);

            return new ApplicationUser(id, userName, passwordValue.Result, email);
        }

        public Operation<ApplicationUser, string> AddRole(Role role) 
        {
            var exists = _roles.FirstOrDefault(x => x.Code == role.Code);

            if (!(exists is null))
                return Operation.Error($"User already has this role! UserName:{UserName}");

            _roles.Add(role);

            return this;
        }

        private static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes).ToLower();
        }

        public bool CheckPassword(string password)
            => Password == PasswordValue.Create(HashPassword(password)).Result;
    }
}
