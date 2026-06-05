using System.Text.Json;
using Terminalia.Core.Assets;

namespace Terminalia.Assets.Json;

public sealed class MapAssetDocument
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public string Name { get; set; } = "first_dungeon";
    public int Width { get; set; }
    public int Height { get; set; }
    public List<string> Tiles { get; set; } = [];
    public Dictionary<string, string> Legend { get; set; } = [];
    public ZoneGridDto? Zones { get; set; }

    public bool HasZones => Zones is not null && Zones.Defs.Count > 0;

    public static MapAssetDocument CreateDefault()
    {
        return new MapAssetDocument
        {
            Name = "first_dungeon",
            Width = 18,
            Height = 7,
            Tiles =
            [
                "##################",
                "#................#",
                "#..........*.....#",
                "#...@#...........#",
                "#................#",
                "#................#",
                "##################"
            ],
            Legend = CreateDefaultLegend()
        };
    }

    public static MapAssetDocument Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Map asset was not found.", path);
        }

        using var stream = File.OpenRead(path);
        var document = JsonSerializer.Deserialize<MapAssetDocument>(stream, JsonOptions)
            ?? throw new InvalidDataException("Map document could not be loaded.");

        document.Validate();
        document.EnsureDefaultLegend();
        return document;
    }

    public void Save(string path)
    {
        Validate();
        EnsureDefaultLegend();

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, this, JsonOptions);
    }

    public bool Contains(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public char GetTile(int x, int y)
    {
        if (!Contains(x, y))
        {
            throw new ArgumentOutOfRangeException(nameof(x), "Coordinates must be inside the map.");
        }

        return Tiles[y][x];
    }

    public bool TryFindTile(char tile, out (int X, int Y) position)
    {
        for (var y = 0; y < Height; y++)
        {
            var x = Tiles[y].IndexOf(tile);
            if (x >= 0)
            {
                position = (x, y);
                return true;
            }
        }
        position = default;
        return false;
    }

    public void SetTile(int x, int y, char tile)
    {
        if (!Contains(x, y))
        {
            return;
        }

        if (tile == '@')
        {
            RemoveExistingPlayerSpawn();
        }

        var row = Tiles[y].ToCharArray();
        row[x] = tile;
        Tiles[y] = new string(row);
        EnsureDefaultLegend();
    }

    public void FloodFill(int x, int y, char tile)
    {
        if (!Contains(x, y))
        {
            return;
        }

        var targetTile = GetTile(x, y);
        if (targetTile == tile)
        {
            return;
        }

        if (tile == '@')
        {
            SetTile(x, y, tile);
            return;
        }

        var pending = new Queue<(int X, int Y)>();
        pending.Enqueue((x, y));
        var visited = 0;
        var maxCells = MaxDimension * MaxDimension;

        while (pending.Count > 0 && visited++ < maxCells)
        {
            var current = pending.Dequeue();
            if (!Contains(current.X, current.Y) || GetTile(current.X, current.Y) != targetTile)
            {
                continue;
            }

            SetTile(current.X, current.Y, tile);

            pending.Enqueue((current.X + 1, current.Y));
            pending.Enqueue((current.X - 1, current.Y));
            pending.Enqueue((current.X, current.Y + 1));
            pending.Enqueue((current.X, current.Y - 1));
        }
    }

    public void Resize(int width, int height, char fillTile = '.')
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var resizedTiles = new List<string>(height);

        for (var y = 0; y < height; y++)
        {
            var row = new char[width];
            Array.Fill(row, fillTile);

            if (y < Tiles.Count)
            {
                var existingRow = Tiles[y];
                var copyLength = Math.Min(width, existingRow.Length);
                existingRow.AsSpan(0, copyLength).CopyTo(row);
            }

            resizedTiles.Add(new string(row));
        }

        Width = width;
        Height = height;
        Tiles = resizedTiles;
        EnsureDefaultLegend();

        if (Zones is not null)
        {
            var keysToRemove = Zones.Map.Keys
                .Where(k => !TryParseCell(k, out var x, out var y) || x >= width || y >= height)
                .ToList();
            foreach (var key in keysToRemove)
            {
                Zones.Map.Remove(key);
            }
        }
    }

    public const int MaxDimension = 4096;

    public TileMap ToTileMap()
    {
        var legend = new Dictionary<char, string>();
        foreach (var item in Legend)
        {
            if (item.Key.Length == 1)
            {
                legend[item.Key[0]] = item.Value;
            }
        }

        return new TileMap(Name, Width, Height, Tiles, legend, ToZoneGrid());
    }

    public ZoneGrid ToZoneGrid()
    {
        if (Zones is null || Zones.Defs.Count == 0)
        {
            return ZoneGrid.Empty;
        }

        var zoneDefs = new Dictionary<string, Zone>();
        foreach (var (name, def) in Zones.Defs)
        {
            zoneDefs[name] = new Zone(name, def.TargetMap);
        }

        var cells = new string?[Width * Height];
        foreach (var (key, zoneName) in Zones.Map)
        {
            if (TryParseCell(key, out var x, out var y) && x >= 0 && x < Width && y >= 0 && y < Height)
            {
                cells[y * Width + x] = zoneName;
            }
        }

        return new ZoneGrid(Width, Height, zoneDefs, cells);
    }

    public void EnsureZoneGrid()
    {
        Zones ??= new ZoneGridDto();
    }

    public void SetZone(int x, int y, string? zoneName)
    {
        if (!Contains(x, y))
        {
            return;
        }

        EnsureZoneGrid();
        var key = $"{x},{y}";
        if (zoneName is null)
        {
            Zones!.Map.Remove(key);
        }
        else
        {
            Zones!.Map[key] = zoneName;
        }
    }

    public void DefineZone(string name, string? targetMap)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        EnsureZoneGrid();
        Zones!.Defs[name] = new ZoneDefDto { TargetMap = string.IsNullOrWhiteSpace(targetMap) ? null : targetMap.Trim() };
    }

    public void RemoveZoneDefinition(string name)
    {
        if (Zones is null)
        {
            return;
        }

        Zones.Defs.Remove(name);

        var keysToRemove = Zones.Map
            .Where(kv => kv.Value == name)
            .Select(kv => kv.Key)
            .ToList();
        foreach (var key in keysToRemove)
        {
            Zones.Map.Remove(key);
        }

        if (Zones.Defs.Count == 0 && Zones.Map.Count == 0)
        {
            Zones = null;
        }
    }

    public IEnumerable<string> GetZoneNames() =>
        Zones?.Defs.Keys ?? Enumerable.Empty<string>();

    public string? GetZoneAt(int x, int y)
    {
        if (Zones is null || !Contains(x, y))
        {
            return null;
        }
        Zones.Map.TryGetValue($"{x},{y}", out var name);
        return name;
    }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Height);

        if (Width > MaxDimension || Height > MaxDimension)
        {
            throw new InvalidDataException($"Map dimensions must not exceed {MaxDimension}x{MaxDimension}.");
        }

        if (Tiles.Count != Height)
        {
            throw new InvalidDataException("Tile row count must match map height.");
        }

        if (Tiles.Any(row => row.Length != Width))
        {
            throw new InvalidDataException("Every tile row must match map width.");
        }
    }

    private static bool TryParseCell(string key, out int x, out int y)
    {
        x = 0;
        y = 0;
        var comma = key.IndexOf(',');
        if (comma < 0)
        {
            return false;
        }
        return int.TryParse(key.AsSpan(0, comma), out x)
            && int.TryParse(key.AsSpan(comma + 1), out y);
    }

    private void RemoveExistingPlayerSpawn()
    {
        for (var y = 0; y < Height; y++)
        {
            Tiles[y] = Tiles[y].Replace('@', '.');
        }
    }

    private void EnsureDefaultLegend()
    {
        Legend ??= [];

        foreach (var item in CreateDefaultLegend())
        {
            Legend.TryAdd(item.Key, item.Value);
        }
    }

    private static Dictionary<string, string> CreateDefaultLegend()
    {
        return new Dictionary<string, string>
        {
            ["#"] = "wall",
            ["."] = "floor",
            ["@"] = "player_spawn",
            ["*"] = "relic"
        };
    }
}
