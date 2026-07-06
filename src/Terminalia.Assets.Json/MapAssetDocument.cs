using System.Text.Json;
using Terminalia.Core.Assets;

namespace Terminalia.Assets.Json;

public sealed partial class MapAssetDocument
{
    public const int MaxDimension = 4096;

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
    public int CameraZoom { get; set; } = TileMap.DefaultCameraZoom;

    public bool HasZones => Zones is not null && Zones.Defs.Count > 0;

    public void SetCameraZoom(int zoom)
    {
        CameraZoom = Math.Clamp(zoom, TileMap.MinCameraZoom, TileMap.MaxCameraZoom);
    }

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

        return new TileMap(Name, Width, Height, Tiles, legend, ToZoneGrid(), CameraZoom, ToEntityProperties());
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

        if (CameraZoom < TileMap.MinCameraZoom || CameraZoom > TileMap.MaxCameraZoom)
        {
            throw new InvalidDataException($"Camera zoom must be between {TileMap.MinCameraZoom} and {TileMap.MaxCameraZoom}.");
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
