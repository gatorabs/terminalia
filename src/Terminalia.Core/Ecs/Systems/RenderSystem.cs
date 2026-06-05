using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;

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

        var offsetX = 0;
        var offsetY = 0;
        var cam = scene.GetEntitiesWithComponent<CameraComponent>()
            .FirstOrDefault()
            ?.GetComponent<CameraComponent>();
        if (cam is not null)
        {
            offsetX = cam.OffsetX;
            offsetY = cam.OffsetY;
        }

        foreach (var item in scene.GetEntities()
            .Select(entity => new
            {
                Transform = entity.GetComponent<TransformComponent>(),
                Render = entity.GetComponent<RenderComponent>()
            })
            .Where(item => item.Transform is not null && item.Render is { IsVisible: true })
            .OrderBy(item => item.Render!.Layer))
        {
            var sx = item.Transform!.Position.X - offsetX;
            var sy = item.Transform.Position.Y - offsetY;

            if (sx < 0 || sx >= _renderer.Width || sy < 0 || sy >= _renderer.Height)
            {
                continue;
            }

            _renderer.Draw(sx, sy, item.Render!.Character);
        }
    }
}
