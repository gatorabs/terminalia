using Terminalia.Core.Abstractions.Ecs;

namespace Terminalia.Core.Ecs;

public sealed class Entity
{
    private readonly Dictionary<Type, IComponent> _components = [];

    public Entity(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = Guid.NewGuid();
        Name = name;
    }

    public Entity(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
    }

    public Guid Id { get; }

    public string Name { get; }

    public void AddComponent(IComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);
        _components[component.GetType()] = component;
    }

    public T? GetComponent<T>() where T : class, IComponent
    {
        return _components.TryGetValue(typeof(T), out var component)
            ? component as T
            : null;
    }

    public bool HasComponent<T>() where T : class, IComponent
    {
        return _components.ContainsKey(typeof(T));
    }

    public void RemoveComponent<T>() where T : class, IComponent
    {
        _components.Remove(typeof(T));
    }
}
