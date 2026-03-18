using Calabonga.Blazor.AppDefinitions;
using System.Reflection;

namespace Catalog.EventMonitor.Blazor.Components;

public partial class Routes
{
    public IEnumerable<Assembly> Assemblies => ModuleDefinitions.Instance.Assemblies;
}
