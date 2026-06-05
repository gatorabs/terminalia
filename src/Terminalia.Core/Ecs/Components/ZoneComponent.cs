using Terminalia.Core.Abstractions.Ecs;

namespace Terminalia.Core.Ecs.Components;

public sealed class ZoneComponent : IComponent
{
    public ZoneComponent(string zoneName, string? targetMap)
    {
        ZoneName = zoneName;
        TargetMap = targetMap;
    }

    public string ZoneName { get; set; }

    public string? TargetMap { get; set; }
}
