using Terminalia.Core.Assets;

namespace Terminalia.Assets.Json;

public sealed partial class MapAssetDocument
{
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
}
