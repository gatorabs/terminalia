using Terminalia.Assets.Json;
using Terminalia.Core.Assets;

namespace Terminalia.Editor.Selection.Descriptors;

public sealed class RelicTileDescriptor : ITileDescriptor
{
    public bool CanDescribe(char tile) => tile == '*';

    public SelectedObjectInfo Describe(MapAssetDocument document, Point cell)
    {
        document.Legend.TryGetValue("*", out var description);

        return new SelectedObjectInfo
        {
            Tile = '*',
            Cell = cell,
            DisplayName = "Relic",
            Description = description,
            Properties =
            [
                DescriptorProperties.ReadOnlyPosition(cell),
                DescriptorProperties.Toggle(
                    EntityPropertyKeys.Collectible,
                    "Collectible",
                    document.GetEntityProperty(cell.X, cell.Y, EntityPropertyKeys.Collectible),
                    EntityPropertyKeys.DefaultCollectible),
                DescriptorProperties.Toggle(
                    EntityPropertyKeys.DisappearOnCollect,
                    "Disappear on collect",
                    document.GetEntityProperty(cell.X, cell.Y, EntityPropertyKeys.DisappearOnCollect),
                    EntityPropertyKeys.DefaultDisappearOnCollect)
            ]
        };
    }

    public void ApplyProperty(MapAssetDocument document, Point cell, string key, string value)
    {
        document.SetEntityProperty(cell.X, cell.Y, key, value);
    }
}
