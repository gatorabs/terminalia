using Terminalia.Assets.Json;
using Terminalia.Core.Assets;

namespace Terminalia.UnitTests.Assets;

public sealed class MapAssetDocumentCameraZoomTests
{
    [Fact]
    public void SetCameraZoom_Should_Keep_Value_Inside_Range()
    {
        var document = MapAssetDocument.CreateDefault();

        document.SetCameraZoom(64);

        Assert.Equal(64, document.CameraZoom);
    }

    [Fact]
    public void SetCameraZoom_Should_Clamp_Below_Minimum()
    {
        var document = MapAssetDocument.CreateDefault();

        document.SetCameraZoom(TileMap.MinCameraZoom - 100);

        Assert.Equal(TileMap.MinCameraZoom, document.CameraZoom);
    }

    [Fact]
    public void SetCameraZoom_Should_Clamp_Above_Maximum()
    {
        var document = MapAssetDocument.CreateDefault();

        document.SetCameraZoom(TileMap.MaxCameraZoom + 100);

        Assert.Equal(TileMap.MaxCameraZoom, document.CameraZoom);
    }
}
