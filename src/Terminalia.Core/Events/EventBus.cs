namespace Terminalia.Core.Events;

public sealed class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = [];

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(TEvent);
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers = [];
            _handlers[eventType] = handlers;
        }

        handlers.Add(handler);
    }

    public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            handlers.Remove(handler);
        }
    }

    public void Publish<TEvent>(TEvent gameEvent) where TEvent : IGameEvent
    {
        ArgumentNullException.ThrowIfNull(gameEvent);

        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        foreach (var handler in handlers.ToArray())
        {
            ((Action<TEvent>)handler)(gameEvent);
        }
    }
}
