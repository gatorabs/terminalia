using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Tests.Ecs;

public sealed class MovementSystemTests
{
    [Fact]
    public void MovementSystem_Should_Update_Position()
    {
        var scene = new Scene("Movement Test");
        var entity = new Entity("Player");
        var transform = new TransformComponent(new Position(2, 3));

        entity.AddComponent(transform);
        entity.AddComponent(new VelocityComponent(new Velocity(1, -1)));
        scene.AddEntity(entity);

        new MovementSystem().Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        Assert.Equal(new Position(3, 2), transform.Position);
    }

    [Fact]
    public void MovementSystem_Should_Skip_Entities_With_ColliderComponent()
    {
        var scene = new Scene("Movement Test");
        var entity = new Entity("Player");
        var transform = new TransformComponent(new Position(2, 3));

        entity.AddComponent(transform);
        entity.AddComponent(new VelocityComponent(new Velocity(1, 0)));
        entity.AddComponent(new ColliderComponent(new Size(1, 1)));
        scene.AddEntity(entity);

        new MovementSystem().Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        Assert.Equal(new Position(2, 3), transform.Position);
    }

    [Fact]
    public void MovementSystem_And_CollisionSystem_Together_Move_Entity_Exactly_One_Tile()
    {
        var scene = new Scene("Combined Test");

        // Entity sem collider: MovementSystem deve mover
        var freeEntity = new Entity("Ghost");
        var freeTransform = new TransformComponent(new Position(0, 0));
        freeEntity.AddComponent(freeTransform);
        freeEntity.AddComponent(new VelocityComponent(new Velocity(1, 0)));
        scene.AddEntity(freeEntity);

        // Entity com collider em espaço livre: CollisionSystem deve mover 1 tile
        var solidEntity = new Entity("Player");
        var solidTransform = new TransformComponent(new Position(5, 5));
        solidEntity.AddComponent(solidTransform);
        solidEntity.AddComponent(new VelocityComponent(new Velocity(1, 0)));
        solidEntity.AddComponent(new ColliderComponent(new Size(1, 1)));
        scene.AddEntity(solidEntity);

        var time = new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1);
        new MovementSystem().Update(scene, time);
        new CollisionSystem().Update(scene, time);

        Assert.Equal(new Position(1, 0), freeTransform.Position);    // moveu 1x
        Assert.Equal(new Position(6, 5), solidTransform.Position);   // moveu 1x, não 2x
    }
}
