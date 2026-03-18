using Microsoft.AspNetCore.Components.Routing;

namespace Catalog.EventMonitor.Blazor.Extensions;

public record AssemblyNavLink(string Route, string Title, NavLinkMatch Match = NavLinkMatch.Prefix);