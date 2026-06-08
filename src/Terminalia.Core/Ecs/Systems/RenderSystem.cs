using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Ecs.Systems;

public sealed class RenderSystem : IGameSystem
{
    private readonly IRenderer _renderer;

    public RenderSystem(IRenderer renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public void Update(Scene scene, GameTime time)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var camera = scene.GetEntitiesWithComponent<CameraComponent>().FirstOrDefault()?.GetComponent<CameraComponent>();

        foreach (var item in scene.GetEntities()
            .Select(entity => new
            {
                Transform = entity.GetComponent<TransformComponent>(),
                Render = entity.GetComponent<RenderComponent>()
            })
            .Where(item => item.Transform is not null && item.Render is { IsVisible: true })
            .OrderBy(item => item.Render!.Layer))
        {
            var screen = ToScreen(item.Transform!.Position, camera);
            if (screen is null)
            {
                continue;
            }

            _renderer.Draw(screen.Value.X, screen.Value.Y, item.Render!.Character);
        }
    }

    private static Position? ToScreen(Position worldPosition, CameraComponent? camera)
    {
        if (camera is null)
        {
            return worldPosition;
        }

        var screen = new Position(worldPosition.X - camera.Offset.X, worldPosition.Y - camera.Offset.Y);
        if (screen.X < 0 || screen.X >= camera.Viewport.Width || screen.Y < 0 || screen.Y >= camera.Viewport.Height)
        {
            return null;
        }

        return screen;
    }
}
