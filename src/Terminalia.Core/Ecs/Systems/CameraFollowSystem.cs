using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Ecs.Systems;

public sealed class CameraFollowSystem : IGameSystem
{
    private readonly Size _mapSize;
    private readonly string _targetName;

    public CameraFollowSystem(Size mapSize, string targetName = "Player")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        _mapSize = mapSize;
        _targetName = targetName;
    }

    public void Update(Scene scene, GameTime time)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var camera = scene.GetEntitiesWithComponent<CameraComponent>().FirstOrDefault()?.GetComponent<CameraComponent>();
        var target = scene.FindEntity(_targetName)?.GetComponent<TransformComponent>();

        if (camera is null || target is null)
        {
            return;
        }

        camera.Offset = ComputeOffset(target.Position, camera.Viewport, _mapSize);
    }

    private static Position ComputeOffset(Position focus, Size viewport, Size mapSize)
    {
        var x = Clamp(focus.X - viewport.Width / 2, mapSize.Width - viewport.Width);
        var y = Clamp(focus.Y - viewport.Height / 2, mapSize.Height - viewport.Height);
        return new Position(x, y);
    }

    private static int Clamp(int value, int max)
    {
        return Math.Clamp(value, 0, Math.Max(0, max));
    }
}
