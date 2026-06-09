using Terminalia.Core.ValueObjects;

namespace Terminalia.Core.Assets;

public sealed class TileMap
{
    private static readonly IReadOnlyDictionary<string, string> EmptyEntityProperties = new Dictionary<string, string>();

    private readonly List<string> _tiles;
    private readonly Dictionary<char, string> _legend;
    private readonly Dictionary<Position, IReadOnlyDictionary<string, string>> _entityProperties;

    public const int DefaultCameraZoom = 48;
    public const int MinCameraZoom = 16;
    public const int MaxCameraZoom = 128;

    public static TileMap Empty { get; } = new("untitled", 1, 1, ["."], new Dictionary<char, string> { ['.'] = "floor" });

    public TileMap(
        string name,
        int width,
        int height,
        IEnumerable<string> tiles,
        IReadOnlyDictionary<char, string> legend,
        ZoneGrid? zones = null,
        int cameraZoom = DefaultCameraZoom,
        IReadOnlyDictionary<Position, IReadOnlyDictionary<string, string>>? entityProperties = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(tiles);
        ArgumentNullException.ThrowIfNull(legend);
        ArgumentOutOfRangeException.ThrowIfLessThan(cameraZoom, MinCameraZoom);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(cameraZoom, MaxCameraZoom);

        Name = name;
        Width = width;
        Height = height;
        _tiles = tiles.ToList();
        _legend = new Dictionary<char, string>(legend);
        Zones = zones ?? ZoneGrid.Empty;
        CameraZoom = cameraZoom;
        _entityProperties = entityProperties is null
            ? []
            : new Dictionary<Position, IReadOnlyDictionary<string, string>>(entityProperties);

        if (_tiles.Count != Height)
        {
            throw new ArgumentException("Tile row count must match map height.", nameof(tiles));
        }

        if (_tiles.Any(row => row.Length != Width))
        {
            throw new ArgumentException("Every tile row must match map width.", nameof(tiles));
        }
    }

    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public IReadOnlyList<string> Tiles => _tiles;
    public IReadOnlyDictionary<char, string> Legend => _legend;
    public ZoneGrid Zones { get; }
    public int CameraZoom { get; }
    public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public char GetTile(int x, int y)
    {
        if (!Contains(x, y))
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Coordinates must be inside the map.");
        }

        return _tiles[y][x];
    }

    public IReadOnlyDictionary<string, string> GetEntityProperties(int x, int y)
    {
        return _entityProperties.TryGetValue(new Position(x, y), out var properties)
            ? properties
            : EmptyEntityProperties;
    }
}
