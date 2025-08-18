using System.Reflection;

namespace Catalog.OrderService.Definitions
{
    public class OrderAssemblyReference
    {
        public readonly Assembly Assembly = typeof(OrderAssemblyReference).Assembly;
    }
}
