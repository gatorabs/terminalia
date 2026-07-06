namespace Terminalia.Assets.Json;

public sealed partial class MapAssetDocument
{
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
        if (!Contains(x, y) || GetTile(x, y) == tile)
        {
            return;
        }

        ClearEntityProperties(x, y);

        if (tile == '@')
        {
            if (TryFindTile('@', out var previousSpawn))
            {
                MoveEntityProperties(previousSpawn.X, previousSpawn.Y, x, y);
            }

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

        RemoveOutOfBoundsEntities(width, height);
    }

    private void RemoveExistingPlayerSpawn()
    {
        for (var y = 0; y < Height; y++)
        {
            Tiles[y] = Tiles[y].Replace('@', '.');
        }
    }
}
