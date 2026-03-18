using Calabonga.Blazor.AppDefinitions;

namespace Catalog.EventMonitor.Blazor.Extensions;

public static class ModulesExtensions
{
    public static IEnumerable<AssemblyNavLink> AsNavLinks(this IEnumerable<BlazorAppDefinition> source)
    {
        return source
            .SelectMany(x => x.Modules)
            .Select(x => new AssemblyNavLink(x.Route, x.Title, x.Match));
    }
}
