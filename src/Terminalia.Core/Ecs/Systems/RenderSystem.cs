using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Rendering;
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
        ArgumentNullException.ThrowIfNull(time);

        foreach (var item in scene.Entities
            .Where(entity => entity.IsActive)
            .Select(entity => new
            {
                Transform = entity.GetComponent<TransformComponent>(),
                Render = entity.GetComponent<RenderComponent>()
            })
            .Where(item => item.Transform is not null && item.Render is { IsVisible: true })
            .OrderBy(item => item.Render!.Layer))
        {
            _renderer.Draw(
                item.Transform!.Position.X,
                item.Transform.Position.Y,
                item.Render!.Character);
        }
    }
}
