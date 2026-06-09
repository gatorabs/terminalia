using Terminalia.Assets.Json;

namespace Terminalia.Editor.Selection.Descriptors;

public sealed class DefaultTileDescriptor : ITileDescriptor
{
    public bool CanDescribe(char tile) => true;

    public SelectedObjectInfo Describe(MapAssetDocument document, Point cell)
    {
        var tile = document.GetTile(cell.X, cell.Y);
        document.Legend.TryGetValue(tile.ToString(), out var description);

        return new SelectedObjectInfo
        {
            Tile = tile,
            Cell = cell,
            DisplayName = description ?? "Tile",
            Description = description,
            Properties = [DescriptorProperties.ReadOnlyPosition(cell)]
        };
    }

    public void ApplyProperty(MapAssetDocument document, Point cell, string key, string value)
    {
    }
}
