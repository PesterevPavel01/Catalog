using System.Reflection;

namespace Catalog.ExchangeService.Definitions
{
    public class ExchangeAssemblyReference
    {
        public readonly Assembly Assembly = typeof(ExchangeAssemblyReference).Assembly;
    }
}
