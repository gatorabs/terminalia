using Terminalia.Core.Abstractions.Ecs;

namespace Terminalia.Core.Ecs.Components;

public sealed class CollectibleComponent : IComponent
{
    public CollectibleComponent(bool disappearOnCollect = true)
    {
        DisappearOnCollect = disappearOnCollect;
    }

    public bool DisappearOnCollect { get; set; }

    public bool IsCollected { get; set; }
}
