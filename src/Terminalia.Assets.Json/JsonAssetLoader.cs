using System.Text.Json;
using Terminalia.Core.Abstractions.Assets;
using Terminalia.Core.Assets;

namespace Terminalia.Assets.Json;

public sealed class JsonAssetLoader : IAssetLoader
{
    private readonly string _assetRoot;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonAssetLoader(string assetRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetRoot);
        _assetRoot = Path.GetFullPath(assetRoot);
    }

    public TileMap LoadMap(string assetName)
    {
        var document = MapAssetDocument.Load(ResolveAssetPath("maps", assetName, ".json"));

        return new TileMap(
            document.Name,
            document.Width,
            document.Height,
            document.Tiles,
            ToCharacterLegend(document.Legend));
    }

    public Sprite LoadSprite(string assetName)
    {
        var path = ResolveAssetPath("sprites", assetName, ".txt");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Sprite asset was not found.", path);
        }

        var lines = File.ReadAllLines(path);
        return new Sprite(assetName, lines);
    }

    public Animation LoadAnimation(string assetName)
    {
        var path = ResolveAssetPath("animations", assetName, ".json");
        var definition = ReadJson<AnimationDefinition>(path);

        if (definition.Name is null)
        {
            throw new InvalidDataException("Animation name is required.");
        }

        if (definition.Frames is null)
        {
            throw new InvalidDataException("Animation frames are required.");
        }

        var frames = definition.Frames
            .Select((lines, index) => new Sprite($"{definition.Name}_frame_{index}", lines))
            .ToList();

        return new Animation(definition.Name, TimeSpan.FromMilliseconds(definition.FrameDurationMs), frames);
    }

    private string ResolveAssetPath(string subdir, string assetName, string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assetName);

        if (assetName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || assetName.Contains('/')
            || assetName.Contains('\\'))
        {
            throw new ArgumentException("Asset name must not contain path separators or invalid characters.", nameof(assetName));
        }

        var candidate = Path.GetFullPath(Path.Combine(_assetRoot, subdir, assetName + extension));
        var root = _assetRoot + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Asset name resolves outside the asset root.", nameof(assetName));
        }

        return candidate;
    }

    private TDefinition ReadJson<TDefinition>(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Asset file was not found.", path);
        }

        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<TDefinition>(stream, _jsonOptions)
            ?? throw new InvalidDataException("Asset JSON could not be deserialized.");
    }

    private static IReadOnlyDictionary<char, string> ToCharacterLegend(IReadOnlyDictionary<string, string> legend)
    {
        var result = new Dictionary<char, string>();

        foreach (var item in legend)
        {
            if (item.Key.Length != 1)
            {
                throw new InvalidDataException("Map legend keys must be a single character.");
            }

            result[item.Key[0]] = item.Value;
        }

        return result;
    }
}
