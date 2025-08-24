using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public class ModuleType : SimpleEntity
    {
        private readonly List<Module> _modules = [];
        protected ModuleType(TitleValue title, string code, Guid id) : base(title, code, id)
        {
        }

        public static Operation<ModuleType, string> Create(string title, string code, Guid? Id = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Operation.Error("Value is empty or null");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Operation.Error("Code is empty or null");
            }

            var titleValue = TitleValue.Create(title);

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            return new ModuleType(titleValue.Result, code,id: Id ?? Guid.Empty);
        }

        public IReadOnlyCollection<Module> Modules => _modules.AsReadOnly();
    }
}
