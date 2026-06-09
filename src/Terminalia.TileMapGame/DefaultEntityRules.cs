using Terminalia.Core.Assets;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.ValueObjects;

namespace Terminalia.TileMapGame;

public static class DefaultEntityRules
{
    public static IReadOnlyList<EntityRule> All => [Player, Wall, Item];

    public static readonly EntityRule Player = new('@', (position, properties) =>
    {
        var entity = new Entity("Player");
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent('@', layer: 2));
        entity.AddComponent(new VelocityComponent(Velocity.Zero));
        entity.AddComponent(new ColliderComponent(new Size(1, 1), isSolid: true));
        entity.AddComponent(new ZoneComponent(string.Empty, null));

        var speed = ReadInt(properties, EntityPropertyKeys.Speed, EntityPropertyKeys.DefaultSpeed, EntityPropertyKeys.MinSpeed, EntityPropertyKeys.MaxSpeed);
        entity.AddComponent(new SpeedComponent(speed));

        var health = ReadInt(properties, EntityPropertyKeys.Health, EntityPropertyKeys.DefaultHealth, EntityPropertyKeys.MinHealth, EntityPropertyKeys.MaxHealth);
        entity.AddComponent(new HealthComponent(health, health));

        return entity;
    });

    public static readonly EntityRule Wall = new('#', (position, _) =>
    {
        var entity = new Entity("Wall");
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent('#', layer: 1));
        entity.AddComponent(new ColliderComponent(new Size(1, 1), isSolid: true));
        return entity;
    });

    public static readonly EntityRule Item = new('*', (position, properties) =>
    {
        var entity = new Entity("Item");
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent('*', layer: 1));

        if (ReadBool(properties, EntityPropertyKeys.Collectible, EntityPropertyKeys.DefaultCollectible))
        {
            var disappear = ReadBool(properties, EntityPropertyKeys.DisappearOnCollect, EntityPropertyKeys.DefaultDisappearOnCollect);
            entity.AddComponent(new CollectibleComponent(disappear));
        }

        return entity;
    });

    private static int ReadInt(
        IReadOnlyDictionary<string, string> properties,
        string key,
        int fallback,
        int minimum,
        int maximum)
    {
        return properties.TryGetValue(key, out var raw) && int.TryParse(raw, out var value)
            ? Math.Clamp(value, minimum, maximum)
            : fallback;
    }

    private static bool ReadBool(IReadOnlyDictionary<string, string> properties, string key, bool fallback)
    {
        return properties.TryGetValue(key, out var raw) && bool.TryParse(raw, out var value) ? value : fallback;
    }
}
