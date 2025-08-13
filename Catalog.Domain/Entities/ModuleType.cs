using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public class ModuleType : SimpleEntity
    {
        private readonly List<Module> _modules = [];
        protected ModuleType(TitleValue title, CodeValue code, Guid id) : base(title, code, id)
        {
        }

        public static Operation<ModuleType, string> Create(string title, string code)
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

            var codeValue = CodeValue.Create(code);

            if (!codeValue.Ok)
                return Operation.Error(codeValue.Error);

            return new ModuleType(titleValue.Result, codeValue.Result, Guid.Empty);
        }

        public IReadOnlyCollection<Module> Modules => _modules.AsReadOnly();
    }
}
