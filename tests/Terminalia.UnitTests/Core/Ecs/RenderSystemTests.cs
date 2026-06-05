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
    public void RenderSystem_Should_Cull_Entities_Outside_Viewport()
    {
        var scene = new Scene("Cull Test");
        var renderer = new FakeRenderer();

        var cam = new Entity("Camera");
        cam.AddComponent(new CameraComponent { OffsetX = 5, OffsetY = 0 });
        scene.AddEntity(cam);

        var entity = new Entity("Wall");
        entity.AddComponent(new TransformComponent(new Position(3, 2)));
        entity.AddComponent(new RenderComponent('#'));
        scene.AddEntity(entity);

        new RenderSystem(renderer).Update(
            scene,
            new GameTime(TimeSpan.FromMilliseconds(16), TimeSpan.FromMilliseconds(16), 1));

        Assert.Empty(renderer.Draws);
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
