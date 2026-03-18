using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using Catalog.ModuleOrderEvents.Interfaces;
using Microsoft.Extensions.Logging;

namespace Catalog.ModuleOrderEvents.ViewModels;

public class EventsViewModel : IEventsViewModel
{
    private readonly IEventStoreService _eventStore;
    private readonly ILogger<EventsViewModel> _logger;
    private List<OrderEventModel>? _events;
    private OrderEventModel? _lastEvent;
    private bool _isConnected = true;
    private IDisposable? _subscription;

    public List<OrderEventModel>? Events => _events;
    public OrderEventModel? LastEvent => _lastEvent;
    public bool IsConnected => _isConnected;

    public event Action? StateChanged;

    public EventsViewModel(
        IEventStoreService eventStore,
        ILogger<EventsViewModel> logger)
    {
        _eventStore = eventStore;

        _logger = logger;

        _subscription = _eventStore.Subscribe(OnEventReceivedAsync);
    }

    public async Task LoadEventsAsync()
    {
        try
        {
            _events = [.. _eventStore.GetRecentEvents(50)];

            _isConnected = true;

            NotifyStateChanged();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки событий");
            _isConnected = false;
        }
    }

    private async Task OnEventReceivedAsync(OrderEventModel newEvent)
    {
        _logger.LogWarning("🚨 СОБЫТИЕ ПОЛУЧЕНО В VIEWMODEL: {Code}", newEvent.OrderCode);
        _logger.LogWarning("   Поток: {ThreadId}", Environment.CurrentManagedThreadId);
        _logger.LogWarning("   Events до добавления: {Count}", _events?.Count ?? 0);

        try
        {
            _logger.LogDebug("📢 Получено новое событие: {OrderCode}", newEvent.OrderCode);

            _lastEvent = newEvent;

            if (_events != null)
            {
                _events.Insert(0, newEvent);
                if (_events.Count > 100)
                    _events.RemoveAt(_events.Count - 1);
            }

            _isConnected = true;

            // ВАЖНО: Используем InvokeAsync для переключения на UI-поток
            // Но ViewModel не имеет доступа к InvokeAsync напрямую
            // Поэтому просто вызываем событие
            NotifyStateChanged();

            // Ждём 2 секунды и убираем подсветку
            await Task.Delay(2000);
            _lastEvent = null;
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке нового события");
        }

        _logger.LogWarning("   NotifyStateChanged вызван");
    }

    public string GetRowClass(OrderEventModel evt) => evt.EventType switch
    {
        "Created" => "table-success",
        "Completed" => "table-primary",
        "Cancelled" => "table-danger",
        "Rejected" => "table-warning",
        "WorkflowCreated" => "table-info",
        "InProduction" => "table-secondary",
        _ => ""
    };

    public string GetBadgeClass(OrderEventModel evt) => evt.EventType switch
    {
        "Created" => "success",
        "Completed" => "primary",
        "Cancelled" => "danger",
        "Rejected" => "warning",
        "WorkflowCreated" => "info",
        "InProduction" => "secondary",
        _ => "secondary"
    };

    public string GetEventIcon(OrderEventModel evt) => evt.EventType switch
    {
        "Created" => "🆕",
        "Completed" => "✅",
        "Cancelled" => "❌",
        "Rejected" => "⚠️",
        "WorkflowCreated" => "⚙️",
        "InProduction" => "🏭",
        _ => "📌"
    };

    private void NotifyStateChanged() => StateChanged?.Invoke();

    public void Dispose()
    {
        _subscription?.Dispose();
    }
}
