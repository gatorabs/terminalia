using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.UnitTests.Core.Ecs;

public sealed class CollisionSystemTests
{
    [Fact]
    public void CollisionSystem_Should_Block_Solid_Collider()
    {
        var scene = new Scene("Collision Test");
        var player = CreateSolidEntity("Player", new Position(1, 1), new Velocity(1, 0));
        var wall = CreateSolidEntity("Wall", new Position(2, 1), Velocity.Zero);

        scene.AddEntity(player);
        scene.AddEntity(wall);

        new CollisionSystem().Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        Assert.Equal(new Position(1, 1), player.GetComponent<TransformComponent>()!.Position);
    }

    [Fact]
    public void CollisionSystem_Should_Move_When_Target_Is_Clear()
    {
        var scene = new Scene("Collision Test");
        var player = CreateSolidEntity("Player", new Position(1, 1), new Velocity(1, 0));
        var wall = CreateSolidEntity("Wall", new Position(4, 1), Velocity.Zero);

        scene.AddEntity(player);
        scene.AddEntity(wall);

        new CollisionSystem().Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        Assert.Equal(new Position(2, 1), player.GetComponent<TransformComponent>()!.Position);
    }

    [Fact]
    public void CollisionSystem_Should_Move_Multiple_Cells_When_Path_Is_Clear()
    {
        var scene = new Scene("Collision Test");
        var player = CreateSolidEntity("Player", new Position(1, 1), new Velocity(3, 0));

        scene.AddEntity(player);

        new CollisionSystem().Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        Assert.Equal(new Position(4, 1), player.GetComponent<TransformComponent>()!.Position);
    }

    [Fact]
    public void CollisionSystem_Should_Stop_Before_Wall_When_Moving_Multiple_Cells()
    {
        var scene = new Scene("Collision Test");
        var player = CreateSolidEntity("Player", new Position(1, 1), new Velocity(3, 0));
        var wall = CreateSolidEntity("Wall", new Position(3, 1), Velocity.Zero);

        scene.AddEntity(player);
        scene.AddEntity(wall);

        new CollisionSystem().Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        Assert.Equal(new Position(2, 1), player.GetComponent<TransformComponent>()!.Position);
    }

    private static Entity CreateSolidEntity(string name, Position position, Velocity velocity)
    {
        var entity = new Entity(name);
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new VelocityComponent(velocity));
        entity.AddComponent(new ColliderComponent(new Size(1, 1)));
        return entity;
    }
}
