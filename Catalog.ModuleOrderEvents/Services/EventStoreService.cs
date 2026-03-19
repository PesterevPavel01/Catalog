using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using System.Collections.Concurrent;

namespace Catalog.ModuleOrderEvents.Services;

public class EventStoreService : IEventStoreService
{
    private readonly ConcurrentQueue<OrderEventModel> _events = [];

    private readonly ConcurrentDictionary<Guid, Func<OrderEventModel, Task>> _subscribers = [];

    public IReadOnlyList<OrderEventModel> GetRecentEvents()
        => [.. _events.Reverse()];

    public void AddEvent(OrderEventModel eventModel)
    {
        _events.Enqueue(eventModel);

        while (_events.Count > 50)
            _events.TryDequeue(out _);

        foreach (var subscriber in _subscribers.Values.ToList())
        {
            subscriber.Invoke(eventModel);
        }
    }

    public IDisposable Subscribe(Func<OrderEventModel, Task> callback)
    {
        var id = Guid.NewGuid();

        _subscribers.TryAdd(id, callback);

        return new Subscription(() =>
        {
            _subscribers.TryRemove(id, out _);
        });
    }

    private class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
        public void Dispose() => _unsubscribe();
    }
}