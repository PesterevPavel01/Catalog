using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Entities.Configurations;
using Catalog.Contracts.Entities.Parameters;
using Catalog.Contracts.Interfaces;
using Catalog.Domain.Entities.Base;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Catalog.Domain.Entities
{
    public class Module : SimpleEntity
    {
        private readonly List<Component> _components = [];
        private readonly List<OrderItem> _orderItems = [];

        private readonly List<ModuleTextParameter> _moduleTextParameters = [];
        private readonly List<ModuleNumericParameter> _moduleNumericParameters = [];

        protected Module(TitleValue title, string code, Guid id) : base(title, code, id){}

        public static Operation<Module, string> Create(
            string title,
            string code,
            ModuleType moduleType,
            IModuleParametersValidator parametersValidator,
            List<ModuleTextParameter>? textParameters = null,
            List<ModuleNumericParameter>? numericParameters = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Operation.Error("Code is empty or null");

            var titleValue = TitleValue.Create(title);

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            if (moduleType is null)
                return Operation.Error("ModuleType not found");

            var module = new Module(titleValue.Result, code, Guid.Empty)
                .SetModuleType(moduleType);

            if (!module.Ok)
                return Operation.Error(module.Error);

            if (textParameters is not null)
                foreach (var item in textParameters)
                {
                    var operationResult = module.Result.AddTextParameter(item);
                    if (!operationResult.Ok)
                        return Operation.Error(operationResult.Error);
                }

            if (numericParameters is not null)
                foreach (var item in numericParameters)
                {
                    var operationResult = module.Result.AddNumericParameter(item);
                    if (!operationResult.Ok)
                        return Operation.Error(operationResult.Error);
                }

            var validationResult = parametersValidator.Validate(module.Result);

            if (!validationResult.Ok)
                return Operation.Error(validationResult.Error);

            return module.Result;
        }

        public Operation<Module, string> Update(
            IModuleParametersValidator parametersValidator,
            List<ModuleTextParameter>? textParameters = null,
            List<ModuleNumericParameter>? numericParameters = null,
            List<Component>? components = null)
        {
            if (textParameters is not null)
                foreach (var item in textParameters)
                {
                    var operationResult = AddTextParameter(item);
                    if (!operationResult.Ok)
                        return Operation.Error(operationResult.Error);
                }

            if (numericParameters is not null)
                foreach (var item in numericParameters)
                {
                    var operationResult = AddNumericParameter(item);
                    if (!operationResult.Ok)
                        return Operation.Error(operationResult.Error);
                }

            var validationResult = parametersValidator.Validate(this);

            if (!validationResult.Ok)
                return Operation.Error(validationResult.Error);

            return this;
        }

        public bool IsCustom => CheckCustomization();

        public IReadOnlyCollection<Component> Components => _components.AsReadOnly();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public IReadOnlyCollection<ModuleTextParameter> ModuleTextParameters => _moduleTextParameters.AsReadOnly();
        public IReadOnlyCollection<ModuleNumericParameter> ModuleNumericParameters => _moduleNumericParameters.AsReadOnly();

        public Guid ModuleTypeId { get; private set; }
        public ModuleType ModuleType { get; private set; } = null!;

        private Operation<Module, string> SetModuleType(ModuleType moduleType)
        {
            ModuleType = moduleType;
            return this;
        }

        public Operation<ModuleDto,string> AddComponent(Component component, IModuleParametersValidator parametersValidator)
        {
            var exists = _components.Find(x => x.Id == component.Id);
            
            if (exists is not null)
                return ConvertToDto();

            if (_components.FirstOrDefault(x => x.ComponentType.Code == component.ComponentType.Code) is not null)
                return Operation.Error("Module already has a component of this type!");

            var checkRequiredParametersResult = parametersValidator.Validate(this, component);

            if (!checkRequiredParametersResult.Ok)
                return Operation.Error(checkRequiredParametersResult.Error);

            _components.Add(component);

            return ConvertToDto();
        }

        public Operation<ModuleDto, string> RemoveComponent(Component component)
        {
            var exists = _components.Find(x => x.Id == component.Id);
            
            if (exists is null)
                return ConvertToDto();

            _components.Remove(component);

            return ConvertToDto();
        }

        public Operation<Module, string> AddTextParameter(ModuleTextParameter textParameter)
        {
            var exists = _moduleTextParameters.FirstOrDefault(x => x.Id == textParameter.Id);

            if (exists is not null && textParameter.Id != Guid.Empty)
                return Operation.Error("the module already has this parameter");

           exists = _moduleTextParameters.FirstOrDefault(x => x.ParameterType == textParameter.ParameterType && x.Value == textParameter.Value);

            if (exists is not null)
                return Operation.Error("the module already has this parameter");

            _moduleTextParameters.Add(textParameter);

            return this;
        }

        public Operation<Module, string> AddNumericParameter(ModuleNumericParameter numericParameter)
        {
            var exists = _moduleNumericParameters.FirstOrDefault(x => x.Id == numericParameter.Id);

            if (exists is not null && numericParameter.Id != Guid.Empty)
                return Operation.Error("the module already has this parameter");

            exists = _moduleNumericParameters.FirstOrDefault(x => x.ParameterType == numericParameter.ParameterType && x.Value == numericParameter.Value);

            if (exists is not null)
                return Operation.Error("the module already has this parameter");

            _moduleNumericParameters.Add(numericParameter);

            return this;
        }

        public Operation<Module, string> RemoveTextParameter(ModuleTextParameter textParameter)
        {
            var exists = _moduleTextParameters.Find(x => x.Id == textParameter.Id);
            
            if (exists is null)
                return Operation.Error("parameter not found!");

            _moduleTextParameters.Remove(textParameter);

            return this;
        }

        public Operation<Module, string> RemoveNumericParameter(ModuleNumericParameter numericParameter)
        {
            var exists = _moduleNumericParameters.Find(x => x.Id == numericParameter.Id);
            if (exists is null)
                return Operation.Error("parameter not found!"); 

            _moduleNumericParameters.Remove(numericParameter);

            return this;
        }

        public ModuleDto ConvertToDto()
            => new()
            {
                ModuleCode = Code,
                ModuleType = ModuleType.Title.Value,
                ModuleTypeCode = ModuleType.Code,
                Components = [.. Components
                    .Select(x => x.ConvertToDto())],
                ModuleNumericParameters = [.. ModuleNumericParameters
                    .Select(x => x.ConvertToDto())],
                ModuleTextParameters = [.. ModuleTextParameters
                    .Select(x => x.ConvertToDto())]
            };

        public static Func<IQueryable<Module>, IIncludableQueryable<Module, object>> IncludeRequiredField()
            =>
            query => query
                .Include(x => x.Components)
                    .ThenInclude(x => x.ComponentType)
                .Include(x => x.Components)
                    .ThenInclude(x => x.ComponentTextParameters)
                        .ThenInclude(x => x.ParameterType)
                .Include(x => x.Components)
                    .ThenInclude(x => x.ComponentNumericParameters)
                        .ThenInclude(x => x.ParameterType)
                .Include(x => x.ModuleType)
                .Include(x => x.ModuleNumericParameters)
                    .ThenInclude(x => x.ParameterType)  
                .Include(x => x.ModuleTextParameters)
                    .ThenInclude(x => x.ParameterType);

        private bool CheckCustomization()
        {
            var customComponents = Components.FirstOrDefault(x => x.IsCostom);
            
            if (customComponents is not null) 
                return true;

            return false;
        }
    }
}
