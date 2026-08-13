using Terminalia.Core.Assets;

namespace Terminalia.Assets.Json;

public sealed class JsonAssetLoader
{
    private readonly string _assetRoot;

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
            ToCharacterLegend(document.Legend),
            document.ToZoneGrid(),
            document.CameraZoom,
            document.ToEntityProperties());
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
