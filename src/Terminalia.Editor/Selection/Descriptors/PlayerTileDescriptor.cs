using Terminalia.Assets.Json;
using Terminalia.Core.Assets;

namespace Terminalia.Editor.Selection.Descriptors;

public sealed class PlayerTileDescriptor : ITileDescriptor
{
    public const string CameraZoomKey = "cameraZoom";

    public bool CanDescribe(char tile) => tile == '@';

    public SelectedObjectInfo Describe(MapAssetDocument document, Point cell)
    {
        document.Legend.TryGetValue("@", out var description);

        return new SelectedObjectInfo
        {
            Tile = '@',
            Cell = cell,
            DisplayName = "Player",
            Description = description,
            ShowKernelPreview = true,
            Properties =
            [
                DescriptorProperties.ReadOnlyPosition(cell),
                DescriptorProperties.Number(
                    EntityPropertyKeys.Speed,
                    "Speed",
                    document.GetEntityProperty(cell.X, cell.Y, EntityPropertyKeys.Speed),
                    EntityPropertyKeys.DefaultSpeed,
                    EntityPropertyKeys.MinSpeed,
                    EntityPropertyKeys.MaxSpeed),
                DescriptorProperties.Number(
                    EntityPropertyKeys.Health,
                    "Health",
                    document.GetEntityProperty(cell.X, cell.Y, EntityPropertyKeys.Health),
                    EntityPropertyKeys.DefaultHealth,
                    EntityPropertyKeys.MinHealth,
                    EntityPropertyKeys.MaxHealth),
                DescriptorProperties.Number(
                    CameraZoomKey,
                    "Camera zoom",
                    document.CameraZoom.ToString(),
                    TileMap.DefaultCameraZoom,
                    TileMap.MinCameraZoom,
                    TileMap.MaxCameraZoom)
            ]
        };
    }

    public void ApplyProperty(MapAssetDocument document, Point cell, string key, string value)
    {
        if (key == CameraZoomKey)
        {
            document.SetCameraZoom(DescriptorProperties.ParseInt(value, document.CameraZoom));
            return;
        }

        document.SetEntityProperty(cell.X, cell.Y, key, value);
    }
}
