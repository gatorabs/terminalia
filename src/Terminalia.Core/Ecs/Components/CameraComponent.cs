using Terminalia.Core.Abstractions.Ecs;

namespace Terminalia.Core.Ecs.Components;

public sealed class CameraComponent : IComponent
{
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
}
