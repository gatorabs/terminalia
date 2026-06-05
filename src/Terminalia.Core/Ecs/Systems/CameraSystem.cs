using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;

namespace Terminalia.Core.Ecs.Systems;

public sealed class CameraSystem : IGameSystem
{
    private readonly int _mapWidth;
    private readonly int _mapHeight;
    private readonly int _viewportWidth;
    private readonly int _viewportHeight;

    public CameraSystem(int mapWidth, int mapHeight, int viewportWidth, int viewportHeight)
    {
        _mapWidth = mapWidth;
        _mapHeight = mapHeight;
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
    }

    public void Update(Scene scene, GameTime time)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var player = scene.FindEntity("Player");
        if (player is null)
        {
            return;
        }

        var transform = player.GetComponent<TransformComponent>();
        if (transform is null)
        {
            return;
        }

        var cameraEntity = scene.GetEntitiesWithComponent<CameraComponent>().FirstOrDefault();
        if (cameraEntity is null)
        {
            return;
        }

        var camera = cameraEntity.GetComponent<CameraComponent>()!;
        var maxOffsetX = Math.Max(0, _mapWidth - _viewportWidth);
        var maxOffsetY = Math.Max(0, _mapHeight - _viewportHeight);

        camera.OffsetX = Math.Clamp(transform.Position.X - _viewportWidth / 2, 0, maxOffsetX);
        camera.OffsetY = Math.Clamp(transform.Position.Y - _viewportHeight / 2, 0, maxOffsetY);
    }
}
