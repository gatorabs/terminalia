using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Ecs;
using Terminalia.Core.Runtime;

namespace Terminalia.Core.Scenes;

public sealed class Scene
{
    private readonly List<Entity> _entities = [];
    private readonly List<IGameSystem> _systems = [];

    public Scene(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }

    public void AddEntity(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_entities.Any(e => e.Id == entity.Id))
        {
            throw new InvalidOperationException($"An entity with id '{entity.Id.Value}' already exists in scene '{Name}'.");
        }

        _entities.Add(entity);
    }

    public void AddSystem(IGameSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        _systems.Add(system);
    }

    public IEnumerable<Entity> GetActiveEntities()
        => _entities.Where(e => e.IsActive);

    public IEnumerable<Entity> GetEntitiesWithComponent<T>() where T : class, IComponent
        => _entities.Where(e => e.IsActive && e.HasComponent<T>());

    public Entity? FindEntity(string name)
        => _entities.FirstOrDefault(e => e.Name == name);

    public void Update(GameTime time)
    {
        foreach (var system in _systems)
        {
            system.Update(this, time);
        }
    }
}
