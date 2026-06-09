using Terminalia.Core.Abstractions.Ecs;

namespace Terminalia.Core.Ecs.Components;

public sealed class HealthComponent : IComponent
{
    public HealthComponent(int current, int maximum)
    {
        Current = current;
        Maximum = maximum;
    }

    public int Current { get; set; }

    public int Maximum { get; set; }
}
