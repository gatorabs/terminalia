using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Ecs.Components;

public sealed class VelocityComponent : IComponent
{
    public VelocityComponent(Velocity velocity)
    {
        Velocity = velocity;
    }

    public Velocity Velocity { get; set; }
}
