using System.Reflection;

namespace Catalog.ApprovalService.Definitions
{
    public class ApprovalAssemblyReference
    {
        public readonly Assembly Assembly = typeof(ApprovalAssemblyReference).Assembly;
    }
}
