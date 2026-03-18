using Catalog.Contracts.Models;

namespace Catalog.ModuleOrderEvents.Interfaces;

public interface IEventsViewModel : IDisposable
{
    List<OrderEventModel>? Events { get; }
    OrderEventModel? LastEvent { get; }
    bool IsConnected { get; }
    Task LoadEventsAsync();
    string GetRowClass(OrderEventModel evt);
    string GetBadgeClass(OrderEventModel evt);
    string GetEventIcon(OrderEventModel evt);

    event Action? StateChanged;
}
