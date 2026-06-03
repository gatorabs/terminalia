namespace Terminalia.Core.Assets;

public sealed class TileMap
{
    private readonly List<string> _tiles;
    private readonly Dictionary<char, string> _legend;

    public static TileMap Empty { get; } = new("untitled", 1, 1, ["."], new Dictionary<char, string> { ['.'] = "floor" });

    public TileMap(string name, int width, int height, IEnumerable<string> tiles, IReadOnlyDictionary<char, string> legend)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(tiles);
        ArgumentNullException.ThrowIfNull(legend);

        Name = name;
        Width = width;
        Height = height;
        _tiles = tiles.ToList();
        _legend = new Dictionary<char, string>(legend);

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

    public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public char GetTile(int x, int y)
    {
        if (!Contains(x, y))
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Coordinates must be inside the map.");
        }

        return _tiles[y][x];
    }
}
