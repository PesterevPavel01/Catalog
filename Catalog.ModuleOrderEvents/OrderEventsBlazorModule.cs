using Calabonga.Blazor.AppDefinitions;

namespace Catalog.ModuleOrderEvents;

public class OrderEventsBlazorModule : BlazorModule
{
    public override string Title => "События";

    public override string Description => "Модуль для отображения событий, произошедших с заказами.";

    public override string Route => "/Events";
}