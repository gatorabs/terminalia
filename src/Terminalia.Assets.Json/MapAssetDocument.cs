using System.Text.Json;

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

        while (pending.Count > 0)
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
    }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Height);

        if (Tiles.Count != Height)
        {
            throw new InvalidDataException("Tile row count must match map height.");
        }

        if (Tiles.Any(row => row.Length != Width))
        {
            throw new InvalidDataException("Every tile row must match map width.");
        }
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
