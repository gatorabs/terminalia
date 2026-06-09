using Terminalia.Core.Ecs;
using Terminalia.Core.ValueObjects;

namespace Terminalia.TileMapGame;

public sealed class EntityRule
{
    private readonly Func<Position, IReadOnlyDictionary<string, string>, Entity> _createEntity;

    public EntityRule(char tile, Func<Position, IReadOnlyDictionary<string, string>, Entity> createEntity)
    {
        ArgumentNullException.ThrowIfNull(createEntity);
        Tile = tile;
        _createEntity = createEntity;
    }

    public char Tile { get; }

    public Entity CreateEntity(Position position, IReadOnlyDictionary<string, string> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        return _createEntity(position, properties);
    }
}
