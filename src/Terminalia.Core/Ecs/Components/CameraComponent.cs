using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Ecs.Components;

public sealed class CameraComponent : IComponent
{
    public CameraComponent(Size viewport)
    {
        Viewport = viewport;
    }

    public Size Viewport { get; set; }

    public Position Offset { get; set; }
}
