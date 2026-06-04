using Terminalia.Core.Abstractions.Ecs;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Ecs.Components;

public sealed class ColliderComponent : IComponent
{
    public ColliderComponent(Size size, bool isSolid = true)
    {
        Size = size;
        IsSolid = isSolid;
    }

    public Size Size { get; set; }

    public bool IsSolid { get; set; }
}
