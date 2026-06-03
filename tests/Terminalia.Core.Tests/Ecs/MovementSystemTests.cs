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
}
