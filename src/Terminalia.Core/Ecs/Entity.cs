namespace Terminalia.Core.Ecs;

public sealed class Entity
{
    private readonly Dictionary<Type, IComponent> _components = [];

    public Entity(string name)
        : this(EntityId.New(), name)
    {
    }

    public Entity(EntityId id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
    }

    public EntityId Id { get; }

    public string Name { get; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<IComponent> Components => _components.Values;

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

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
