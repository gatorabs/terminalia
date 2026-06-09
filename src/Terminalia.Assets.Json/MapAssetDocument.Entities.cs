using Terminalia.Core.ValueObjects;

namespace Terminalia.Assets.Json;

public sealed partial class MapAssetDocument
{
    public Dictionary<string, Dictionary<string, string>>? Entities { get; set; }

    public string? GetEntityProperty(int x, int y, string key)
    {
        if (Entities is null || !Entities.TryGetValue(CellKey(x, y), out var properties))
        {
            return null;
        }

        properties.TryGetValue(key, out var value);
        return value;
    }

    public void SetEntityProperty(int x, int y, string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        if (!Contains(x, y))
        {
            return;
        }

        Entities ??= [];
        if (!Entities.TryGetValue(CellKey(x, y), out var properties))
        {
            properties = [];
            Entities[CellKey(x, y)] = properties;
        }

        properties[key] = value;
    }

    public void ClearEntityProperties(int x, int y)
    {
        Entities?.Remove(CellKey(x, y));
        NormalizeEntities();
    }

    public IReadOnlyDictionary<Position, IReadOnlyDictionary<string, string>> ToEntityProperties()
    {
        var result = new Dictionary<Position, IReadOnlyDictionary<string, string>>();
        if (Entities is null)
        {
            return result;
        }

        foreach (var (key, properties) in Entities)
        {
            if (TryParseCell(key, out var x, out var y) && Contains(x, y) && properties.Count > 0)
            {
                result[new Position(x, y)] = properties;
            }
        }

        return result;
    }

    private void MoveEntityProperties(int fromX, int fromY, int toX, int toY)
    {
        if (Entities is null || !Entities.TryGetValue(CellKey(fromX, fromY), out var properties))
        {
            return;
        }

        Entities.Remove(CellKey(fromX, fromY));
        Entities[CellKey(toX, toY)] = properties;
    }

    private void RemoveOutOfBoundsEntities(int width, int height)
    {
        if (Entities is null)
        {
            return;
        }

        var keysToRemove = Entities.Keys
            .Where(k => !TryParseCell(k, out var x, out var y) || x >= width || y >= height)
            .ToList();
        foreach (var key in keysToRemove)
        {
            Entities.Remove(key);
        }

        NormalizeEntities();
    }

    private void NormalizeEntities()
    {
        if (Entities is { Count: 0 })
        {
            Entities = null;
        }
    }

    private static string CellKey(int x, int y) => $"{x},{y}";
}
