using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Catalog.ModuleOrderEvents.Components;

public class EventTableComponentModel : ComponentBase, IDisposable
{
    [Inject] private IEventStoreService EventStoreService { get; set; } = null!;

    [Inject] private ILogger<EventTableComponentModel> Logger { get; set; } = null!;

    protected List<String> Errors { get; set; } = [];

    public List<OrderEventModel> Events { get; set; } = [];

    private IDisposable? _subscription;

    private bool _disposed;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _subscription = EventStoreService.Subscribe(OnEventReceivedAsync);

        LoadEvents();
    }

    private void LoadEvents()
    {
        Errors!.Clear();

        try
        {
            var existingEvents = EventStoreService.GetRecentEvents();

            Events.Clear();

            Events.AddRange(existingEvents.OrderByDescending(x => x.OccurredAt));

        }
        catch (Exception exception)
        {
            Errors.Add($"Ошибка загрузки событий! {exception.Message}");
            Logger.LogError(exception, "Ошибка загрузки событий");
        }

        if (!Events.Any())
            Errors.Add("Не найдено ни одного события!");
    }

    private async Task OnEventReceivedAsync(OrderEventModel newEvent)
    {
        Errors!.Clear();

        Logger.LogDebug("📢 Получено новое событие: {OrderCode}", newEvent.OrderCode);

        Events.Add(newEvent);

        while (Events.Count > 50)
            Events.RemoveAt(Events.Count - 1);

        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _subscription?.Dispose();
    }
}