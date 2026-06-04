using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Runtime;

public static class DefaultEntityRules
{
    public static IReadOnlyList<EntityRule> All => [Player, Wall, Item];

    public static readonly EntityRule Player = new('@', position =>
    {
        var entity = new Entity("Player");
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent('@', layer: 2));
        entity.AddComponent(new VelocityComponent(Velocity.Zero));
        entity.AddComponent(new ColliderComponent(new Size(1, 1), isSolid: true));
        return entity;
    });

    public static readonly EntityRule Wall = new('#', position =>
    {
        var entity = new Entity("Wall");
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent('#', layer: 1));
        entity.AddComponent(new ColliderComponent(new Size(1, 1), isSolid: true));
        return entity;
    });

    public static readonly EntityRule Item = new('*', position =>
    {
        var entity = new Entity("Item");
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent('*', layer: 1));
        return entity;
    });
}
