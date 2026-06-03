using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Ecs.Components;

public sealed class TransformComponent : IComponent
{
    public TransformComponent(Position position)
    {
        Position = position;
    }

    public Position Position { get; set; }
}
