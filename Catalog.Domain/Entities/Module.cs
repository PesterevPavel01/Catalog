using Calabonga.OperationResults;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities
{
    public class Module : SimpleEntity
    {
        private readonly List<Component> _components = [];
        private readonly List<OrderItem> _orderItems = [];

        protected Module(TitleValue title, CodeValue code, Guid id) : base(title, code, id)
        {
        }

        public static Operation<Module, string> Create(string title, string code, ModuleType moduleType)
        {
            if (moduleType is null)
                return Operation.Error("ComponentTupe not found");

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

            return new Module(titleValue.Result, codeValue.Result, Guid.Empty).SetModuleType(moduleType);
        }

        public IReadOnlyCollection<Component> Components => _components.AsReadOnly();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public Guid ModuleTypeId { get; private set; }
        public ModuleType ModuleType { get; private set; } = null!;

        private Operation<Module, string> SetModuleType(ModuleType moduleType)
        {
            ModuleType = moduleType;
            return this;
        }

        public void AddComponent(Component component)
        {
            var exists = _components.Find(x => x.Id == component.Id);
            if (exists is not null)
                return;

            _components.Add(component);
        }

        public void RemoveComponent(Component component)
        {
            var exists = _components.Find(x => x.Id == component.Id);
            if (exists is null)
                return;

            _components.Remove(component);
        }
    }
}
