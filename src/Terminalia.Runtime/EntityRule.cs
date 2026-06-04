using Terminalia.Core.Ecs;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Runtime;

public sealed class EntityRule
{
    public EntityRule(char tile, Func<Position, Entity> createEntity)
    {
        ArgumentNullException.ThrowIfNull(createEntity);
        Tile = tile;
        CreateEntity = createEntity;
    }

    public char Tile { get; }

    public Func<Position, Entity> CreateEntity { get; }
}
