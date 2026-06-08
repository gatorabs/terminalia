using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.UnitTests.Core.Ecs;

public sealed class RenderSystemTests
{
    [Fact]
    public void RenderSystem_Should_Draw_Visible_Entities()
    {
        var scene = new Scene("Render Test");
        var renderer = new FakeRenderer();
        var entity = new Entity("Player");
        entity.AddComponent(new TransformComponent(new Position(2, 3)));
        entity.AddComponent(new RenderComponent('@'));
        scene.AddEntity(entity);

        new RenderSystem(renderer).Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        var draw = Assert.Single(renderer.Draws);
        Assert.Equal((2, 3, '@'), draw);
    }

    [Fact]
    public void RenderSystem_Should_Translate_By_Camera_Offset()
    {
        var scene = new Scene("Render Test");
        var renderer = new FakeRenderer();

        var entity = new Entity("Player");
        entity.AddComponent(new TransformComponent(new Position(12, 8)));
        entity.AddComponent(new RenderComponent('@'));
        scene.AddEntity(entity);

        var camera = new Entity("Camera");
        camera.AddComponent(new CameraComponent(new Size(10, 10)) { Offset = new Position(10, 5) });
        scene.AddEntity(camera);

        new RenderSystem(renderer).Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        var draw = Assert.Single(renderer.Draws);
        Assert.Equal((2, 3, '@'), draw);
    }

    [Fact]
    public void RenderSystem_Should_Cull_Entities_Outside_Camera_Viewport()
    {
        var scene = new Scene("Render Test");
        var renderer = new FakeRenderer();

        var inside = new Entity("Inside");
        inside.AddComponent(new TransformComponent(new Position(11, 11)));
        inside.AddComponent(new RenderComponent('@'));
        scene.AddEntity(inside);

        var outside = new Entity("Outside");
        outside.AddComponent(new TransformComponent(new Position(0, 0)));
        outside.AddComponent(new RenderComponent('#'));
        scene.AddEntity(outside);

        var camera = new Entity("Camera");
        camera.AddComponent(new CameraComponent(new Size(10, 10)) { Offset = new Position(10, 10) });
        scene.AddEntity(camera);

        new RenderSystem(renderer).Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        var draw = Assert.Single(renderer.Draws);
        Assert.Equal((1, 1, '@'), draw);
    }

    private sealed class FakeRenderer : IRenderer
    {
        public int Width => 80;

        public int Height => 25;

        public List<(int X, int Y, char Character)> Draws { get; } = [];

        public void Clear()
        {
        }

        public void Draw(int x, int y, char character)
        {
            Draws.Add((x, y, character));
        }

        public void DrawText(int x, int y, string text)
        {
        }

        public void Present()
        {
        }
    }
}
