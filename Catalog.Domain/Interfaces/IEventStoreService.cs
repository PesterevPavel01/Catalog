using Catalog.Contracts.Models;

namespace Catalog.Contracts.Interfaces;

public interface IEventStoreService
{
    IReadOnlyList<OrderEventModel> GetRecentEvents();
    void AddEvent(OrderEventModel eventModel);
    IDisposable Subscribe(Func<OrderEventModel, Task> callback);
}
