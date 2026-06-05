namespace Terminalia.Core.Assets;

public sealed class ZoneGrid
{
    private static readonly ZoneGrid _empty = new(0, 0, new Dictionary<string, Zone>(), []);
    private readonly IReadOnlyList<string?> _cells;

    public static ZoneGrid Empty => _empty;

    public ZoneGrid(int width, int height, IReadOnlyDictionary<string, Zone> zones, IReadOnlyList<string?> cells)
    {
        Width = width;
        Height = height;
        Zones = zones;
        _cells = cells;
    }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyDictionary<string, Zone> Zones { get; }

    public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public Zone? GetZone(int x, int y)
    {
        if (!Contains(x, y))
        {
            return null;
        }

        var name = _cells[y * Width + x];
        if (name is null || !Zones.TryGetValue(name, out var zone))
        {
            return null;
        }

        return zone;
    }
}
