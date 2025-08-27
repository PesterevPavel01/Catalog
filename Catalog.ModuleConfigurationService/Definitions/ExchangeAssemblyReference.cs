using System.Reflection;

namespace Catalog.ModuleConfigurationService.Definitions
{
    public class ModuleConfigurationAssemblyReference
    {
        public readonly Assembly Assembly = typeof(ModuleConfigurationAssemblyReference).Assembly;
    }
}
