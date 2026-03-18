using Catalog.Contracts.Interfaces;
using Catalog.Contracts.Models;
using System.Collections.Concurrent;

namespace Catalog.ModuleOrderEvents.Services;

public class EventStoreService : IEventStoreService
{
    private readonly ConcurrentQueue<OrderEventModel> _events = new();
    private event Func<OrderEventModel, Task>? _subscribers;

    public IReadOnlyList<OrderEventModel> GetRecentEvents(int count = 50)
        => [.. _events.Reverse().Take(count)];

    public void AddEvent(OrderEventModel eventModel)
    {
        _events.Enqueue(eventModel);

        while (_events.Count > 100)
            _events.TryDequeue(out _);

        // Вызываем подписчиков (их будет 1 - ViewModel)
        _subscribers?.Invoke(eventModel);
    }

    public IDisposable Subscribe(Func<OrderEventModel, Task> callback)
    {
        _subscribers += callback;
        return new Subscription(() => _subscribers -= callback);
    }

    private class Subscription : IDisposable
    {
        private readonly Action _unsubscribe;
        public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;
        public void Dispose() => _unsubscribe();
    }
}