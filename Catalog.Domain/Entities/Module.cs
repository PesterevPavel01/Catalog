using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Module;
using Catalog.Contracts.Entities.Parameters;
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
            List<ModuleRequaredParameter> requaredParameters,
            List<ModuleTextParameter>? textParameters = null,
            List<ModuleNumericParameter>? numericParameters = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Operation.Error("Code is empty or null");

            var titleValue = TitleValue.Create(title);

            if (!titleValue.Ok)
                return Operation.Error(titleValue.Error);

            if (moduleType is null)
                return Operation.Error("ModuleTupe not found");

            if (requaredParameters is null)
                return Operation.Error("RequaredParameters not found");

            var module = new Module(titleValue.Result, code, Guid.Empty)
                .SetModuleType(moduleType);

            if (!module.Ok)
                return Operation.Error(module.Error);

            var moduleRequaredParameters = requaredParameters.FirstOrDefault(x => x.ModuleType == moduleType.Title.Value);

            if (moduleRequaredParameters == null)
                return Operation.Error("ModuleRequaredParameters not found");

            module.Result.ModuleRequaredParameters = moduleRequaredParameters;

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

            var checkRequaredParametersResult = module.Result.CheckRequaredParameters();

            if (!checkRequaredParametersResult.Ok)
                return Operation.Error(checkRequaredParametersResult.Error);

            return module.Result;
        }

        public Operation<Module, string> Update(
            List<ModuleRequaredParameter> requaredParameters,
            List<ModuleTextParameter>? textParameters = null,
            List<ModuleNumericParameter>? numericParameters = null,
            List<Component>? components = null)
        {
            var moduleRequaredParameters = requaredParameters.FirstOrDefault(x => x.ModuleType == ModuleType.Title.Value);

            if (moduleRequaredParameters == null)
                return Operation.Error("ModuleRequaredParameters not found");

            ModuleRequaredParameters = moduleRequaredParameters;

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

            var checkRequaredParametersResult = CheckRequaredParameters();

            if (!checkRequaredParametersResult.Ok)
                return Operation.Error(checkRequaredParametersResult.Error);

            return this;
        }

        public bool IsCustom => CheckCostomization();

        public IReadOnlyCollection<Component> Components => _components.AsReadOnly();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public IReadOnlyCollection<ModuleTextParameter> ModuleTextParameters => _moduleTextParameters.AsReadOnly();
        public IReadOnlyCollection<ModuleNumericParameter> ModuleNumericParameters => _moduleNumericParameters.AsReadOnly();

        public ModuleRequaredParameter ModuleRequaredParameters = null!;

        public Guid ModuleTypeId { get; private set; }
        public ModuleType ModuleType { get; private set; } = null!;

        private Operation<Module, string> SetModuleType(ModuleType moduleType)
        {
            ModuleType = moduleType;
            return this;
        }

        public Operation<ModuleDto,string> AddComponent(Component component)
        {
            var exists = _components.Find(x => x.Id == component.Id);
            
            if (exists is not null)
                return ConvertToDto();

            if (_components.FirstOrDefault(x => x.ComponentType.Code == component.ComponentType.Code) is not null)
                return Operation.Error("Module already has a component of this type!");

            var checkRequaredParametersResult = CheckRequaredParameters(component);

            if (!checkRequaredParametersResult.Ok)
                return Operation.Error(checkRequaredParametersResult.Error);

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

        private Operation<bool, string> CheckRequaredParameters(Component? component = null)
        {
            //проверяем наличие параметров, которые должны быть при создании
            var requaredParameters = ModuleRequaredParameters.Parameters.Where(x => x.Dependencies is null).ToList();

            if (requaredParameters.Count == 0)
                return Operation.Error("Default ModuleRequaredParameters not found");

            if (requaredParameters
                .Select(x => x.Parameter)
                .FirstOrDefault(x =>
                (
                    ModuleNumericParameters is null || !ModuleNumericParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                    && (ModuleTextParameters is null || !ModuleTextParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                ) is not null)
                return Operation.Error("Required parameters are missing from the module");

            if (component is null)
                return true;

            //проверяем параметры, которые необходимы для компонента
            requaredParameters = [..ModuleRequaredParameters.Parameters
                .Where(x => x.Dependencies is not null).ToList()];

            requaredParameters = [..requaredParameters
                .Where(x => x.Dependencies
                    .Select(x => x.ComponentsTypeTitle).Contains(component.ComponentType.Title.Value))];

            if (requaredParameters.Count == 0)
                return true;

            requaredParameters = [.. requaredParameters
                .Where(x => x.Dependencies
                    .Select(x => x.ComponentsTitle).Contains(component.Title.Value))];

            if (requaredParameters.Count == 0)
                return true;

            if (requaredParameters
                .Select(x => x.Parameter)
                .FirstOrDefault(x =>
                (
                    ModuleNumericParameters is null || !ModuleNumericParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                    && (ModuleTextParameters is null || !ModuleTextParameters.Select(x => x.ParameterType.Title.Value).Contains(x))
                ) is not null)
                return Operation.Error("Required parameters are missing from the module");

            return true;
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

        public static Func<IQueryable<Module>, IIncludableQueryable<Module, object>> IncludeRequaredField()
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

        private bool CheckCostomization()
        {
            var customComponents = Components.FirstOrDefault(x => x.IsCostom);
            
            if (customComponents is not null) 
                return true;

            return false;
        }
    }
}
