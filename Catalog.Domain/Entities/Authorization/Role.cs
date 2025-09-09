using Calabonga.OperationResults;
using Catalog.Domain.Entities;
using Catalog.Domain.Entities.Autorization;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Contracts.Entities.Authorization
{
    public class Role : SimpleEntity
    {
        private readonly List<ApplicationUser> _applicationUsers = [];

        private Role(TitleValue title, string code, Guid id) : base(title, code, id)
        {
        }

        public static Operation<Role, string> Create(Guid id, string title, string code)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Operation.Error("Title is empty or null");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Operation.Error("Code is empty or null");
            }

            var titleResult = TitleValue.Create(title);

            if (!titleResult.Ok)
                return Operation.Error(titleResult.Error);

            return new Role(titleResult.Result, code, id);
        }

        public IReadOnlyCollection<ApplicationUser> ApplicationUsers => _applicationUsers.AsReadOnly();
    }
}
