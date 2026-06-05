using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.Assets;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;

namespace Terminalia.Core.Ecs.Systems;

public sealed class ZoneTransitionSystem : IGameSystem
{
    private readonly ZoneGrid _zoneGrid;
    private readonly Action<string> _onTransition;

    public ZoneTransitionSystem(ZoneGrid zoneGrid, Action<string> onTransition)
    {
        _zoneGrid = zoneGrid;
        _onTransition = onTransition;
    }

    public void Update(Scene scene, GameTime time)
    {
        ArgumentNullException.ThrowIfNull(scene);

        foreach (var entity in scene.GetEntities())
        {
            var transform = entity.GetComponent<TransformComponent>();
            var zoneComp = entity.GetComponent<ZoneComponent>();

            if (transform is null || zoneComp is null)
            {
                continue;
            }

            var current = _zoneGrid.GetZone(transform.Position.X, transform.Position.Y);
            var newName = current?.Name ?? string.Empty;

            if (zoneComp.ZoneName == newName)
            {
                continue;
            }

            zoneComp.ZoneName = newName;
            zoneComp.TargetMap = current?.TargetMap;

            if (zoneComp.TargetMap is not null)
            {
                _onTransition(zoneComp.TargetMap);
                return;
            }
        }
    }
}
