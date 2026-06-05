using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.UnitTests.Core.Ecs;

public sealed class CameraSystemTests
{
    private static readonly GameTime AnyTime = new(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1);

    private static Scene BuildScene(Position playerPos)
    {
        var scene = new Scene("Test");

        var player = new Entity("Player");
        player.AddComponent(new TransformComponent(playerPos));
        player.AddComponent(new VelocityComponent(Velocity.Zero));
        scene.AddEntity(player);

        var cam = new Entity("Camera");
        cam.AddComponent(new CameraComponent());
        scene.AddEntity(cam);

        return scene;
    }

    [Fact]
    public void CameraSystem_Should_Center_On_Player()
    {
        var scene = BuildScene(new Position(10, 10));
        new CameraSystem(20, 20, 10, 10).Update(scene, AnyTime);

        var camera = scene.GetEntitiesWithComponent<CameraComponent>()
            .First().GetComponent<CameraComponent>()!;

        Assert.Equal(5, camera.OffsetX);
        Assert.Equal(5, camera.OffsetY);
    }

    [Fact]
    public void CameraSystem_Should_Clamp_At_TopLeft_Edge()
    {
        var scene = BuildScene(new Position(1, 1));
        new CameraSystem(20, 20, 10, 10).Update(scene, AnyTime);

        var camera = scene.GetEntitiesWithComponent<CameraComponent>()
            .First().GetComponent<CameraComponent>()!;

        Assert.Equal(0, camera.OffsetX);
        Assert.Equal(0, camera.OffsetY);
    }

    [Fact]
    public void CameraSystem_Should_Clamp_At_BottomRight_Edge()
    {
        var scene = BuildScene(new Position(19, 19));
        new CameraSystem(20, 20, 10, 10).Update(scene, AnyTime);

        var camera = scene.GetEntitiesWithComponent<CameraComponent>()
            .First().GetComponent<CameraComponent>()!;

        Assert.Equal(10, camera.OffsetX);
        Assert.Equal(10, camera.OffsetY);
    }

    [Fact]
    public void CameraSystem_Should_Keep_Zero_Offset_When_Map_Fits_In_Viewport()
    {
        var scene = BuildScene(new Position(2, 2));
        new CameraSystem(5, 5, 10, 10).Update(scene, AnyTime);

        var camera = scene.GetEntitiesWithComponent<CameraComponent>()
            .First().GetComponent<CameraComponent>()!;

        Assert.Equal(0, camera.OffsetX);
        Assert.Equal(0, camera.OffsetY);
    }

    [Fact]
    public void CameraSystem_Should_Be_Noop_When_No_Player()
    {
        var scene = new Scene("Test");
        var cam = new Entity("Camera");
        cam.AddComponent(new CameraComponent());
        scene.AddEntity(cam);

        new CameraSystem(20, 20, 10, 10).Update(scene, AnyTime);

        var camera = scene.GetEntitiesWithComponent<CameraComponent>()
            .First().GetComponent<CameraComponent>()!;

        Assert.Equal(0, camera.OffsetX);
        Assert.Equal(0, camera.OffsetY);
    }

    [Fact]
    public void CameraSystem_Should_Be_Noop_When_No_CameraEntity()
    {
        var scene = new Scene("Test");
        var player = new Entity("Player");
        player.AddComponent(new TransformComponent(new Position(10, 10)));
        player.AddComponent(new VelocityComponent(Velocity.Zero));
        scene.AddEntity(player);

        var ex = Record.Exception(() => new CameraSystem(20, 20, 10, 10).Update(scene, AnyTime));
        Assert.Null(ex);
    }
}
