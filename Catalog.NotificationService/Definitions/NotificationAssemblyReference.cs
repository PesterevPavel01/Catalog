using System.Reflection;

namespace Catalog.NotificationService.Definitions;

public class NotificationAssemblyReference
{
    public readonly Assembly Assembly = typeof(NotificationAssemblyReference).Assembly;
}
