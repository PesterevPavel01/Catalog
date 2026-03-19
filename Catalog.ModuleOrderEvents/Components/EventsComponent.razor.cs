using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Catalog.ModuleOrderEvents.Components;

public class EventsComponentModel : ComponentBase
{
    [Inject] private ILogger<EventsComponentModel> Logger { get; set; } = null!;

    protected string? Title => "События заказов";  
}
