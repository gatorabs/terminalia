using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.UnitTests.Core.Ecs;

public sealed class CameraFollowSystemTests
{
    private static readonly GameTime AnyTime = new(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1);

    [Fact]
    public void Should_Center_Viewport_On_Target()
    {
        var scene = BuildScene(new Position(10, 10), new Size(10, 10));

        new CameraFollowSystem(new Size(20, 20)).Update(scene, AnyTime);

        Assert.Equal(new Position(5, 5), GetCamera(scene).Offset);
    }

    [Fact]
    public void Should_Clamp_Offset_At_TopLeft_Edge()
    {
        var scene = BuildScene(new Position(1, 1), new Size(10, 10));

        new CameraFollowSystem(new Size(20, 20)).Update(scene, AnyTime);

        Assert.Equal(new Position(0, 0), GetCamera(scene).Offset);
    }

    [Fact]
    public void Should_Clamp_Offset_At_BottomRight_Edge()
    {
        var scene = BuildScene(new Position(19, 19), new Size(10, 10));

        new CameraFollowSystem(new Size(20, 20)).Update(scene, AnyTime);

        Assert.Equal(new Position(10, 10), GetCamera(scene).Offset);
    }

    [Fact]
    public void Should_Keep_Zero_Offset_When_Map_Smaller_Than_Viewport()
    {
        var scene = BuildScene(new Position(2, 2), new Size(10, 10));

        new CameraFollowSystem(new Size(5, 5)).Update(scene, AnyTime);

        Assert.Equal(new Position(0, 0), GetCamera(scene).Offset);
    }

    [Fact]
    public void Should_Be_NoOp_When_Target_Missing()
    {
        var scene = new Scene("Test");
        var camera = new Entity("Camera");
        camera.AddComponent(new CameraComponent(new Size(10, 10)));
        scene.AddEntity(camera);

        var exception = Record.Exception(() => new CameraFollowSystem(new Size(20, 20)).Update(scene, AnyTime));

        Assert.Null(exception);
        Assert.Equal(default, GetCamera(scene).Offset);
    }

    [Fact]
    public void Should_Be_NoOp_When_Camera_Missing()
    {
        var scene = new Scene("Test");
        var player = new Entity("Player");
        player.AddComponent(new TransformComponent(new Position(10, 10)));
        scene.AddEntity(player);

        var exception = Record.Exception(() => new CameraFollowSystem(new Size(20, 20)).Update(scene, AnyTime));

        Assert.Null(exception);
    }

    private static Scene BuildScene(Position playerPosition, Size viewport)
    {
        var scene = new Scene("Test");

        var player = new Entity("Player");
        player.AddComponent(new TransformComponent(playerPosition));
        scene.AddEntity(player);

        var camera = new Entity("Camera");
        camera.AddComponent(new CameraComponent(viewport));
        scene.AddEntity(camera);

        return scene;
    }

    private static CameraComponent GetCamera(Scene scene) =>
        scene.GetEntitiesWithComponent<CameraComponent>().First().GetComponent<CameraComponent>()!;
}
