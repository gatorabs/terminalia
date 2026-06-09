using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;

namespace Terminalia.Core.Ecs.Systems;

public sealed class CollectSystem : IGameSystem
{
    private readonly string _collectorName;

    public CollectSystem(string collectorName = "Player")
    {
        _collectorName = collectorName;
    }

    public void Update(Scene scene, GameTime time)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var collectorTransform = scene.FindEntity(_collectorName)?.GetComponent<TransformComponent>();
        if (collectorTransform is null)
        {
            return;
        }

        foreach (var entity in scene.GetEntitiesWithComponent<CollectibleComponent>())
        {
            var collectible = entity.GetComponent<CollectibleComponent>()!;
            var transform = entity.GetComponent<TransformComponent>();

            if (collectible.IsCollected || transform is null || transform.Position != collectorTransform.Position)
            {
                continue;
            }

            collectible.IsCollected = true;

            if (collectible.DisappearOnCollect && entity.GetComponent<RenderComponent>() is { } render)
            {
                render.IsVisible = false;
            }
        }
    }
}
