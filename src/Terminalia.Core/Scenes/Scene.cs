using Terminalia.Core.Ecs;
using Terminalia.Core.Domain;
using Terminalia.Core.Runtime;

namespace Terminalia.Core.Scenes;

public sealed class Scene : IAggregateRoot
{
    private readonly List<Entity> _entities = [];
    private readonly List<IGameSystem> _systems = [];

    public Scene(string name)
        : this(SceneId.New(), name)
    {
    }

    public Scene(SceneId id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name;
    }

    public SceneId Id { get; }

    public string Name { get; }

    public IReadOnlyList<Entity> Entities => _entities;

    public IReadOnlyList<IGameSystem> Systems => _systems;

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

    public void Update(GameTime time)
    {
        ArgumentNullException.ThrowIfNull(time);

        foreach (var system in _systems)
        {
            system.Update(this, time);
        }
    }
}
