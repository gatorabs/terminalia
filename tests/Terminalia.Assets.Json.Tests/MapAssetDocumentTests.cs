using Terminalia.Assets.Json;

namespace Terminalia.Assets.Json.Tests;

public sealed class MapAssetDocumentTests : IDisposable
{
    private readonly string _tempDirectory;

    public MapAssetDocumentTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"terminalia-map-doc-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void MapAssetDocument_Should_Create_Default_Editable_Map()
    {
        var document = MapAssetDocument.CreateDefault();

        Assert.Equal("first_dungeon", document.Name);
        Assert.Equal(18, document.Width);
        Assert.Equal(7, document.Height);
        Assert.Equal('@', document.GetTile(4, 3));
        Assert.Equal("player_spawn", document.Legend["@"]);
    }

    [Fact]
    public void MapAssetDocument_Should_Save_And_Load_Map()
    {
        var path = Path.Combine(_tempDirectory, "saved_map.json");
        var document = MapAssetDocument.CreateDefault();
        document.SetTile(1, 1, '*');

        document.Save(path);
        var loaded = MapAssetDocument.Load(path);

        Assert.Equal('*', loaded.GetTile(1, 1));
        Assert.Equal(document.Width, loaded.Width);
        Assert.Equal(document.Height, loaded.Height);
    }

    [Fact]
    public void MapAssetDocument_Should_Keep_Only_One_Player_Spawn()
    {
        var document = MapAssetDocument.CreateDefault();

        document.SetTile(2, 2, '@');

        Assert.Equal('.', document.GetTile(4, 3));
        Assert.Equal('@', document.GetTile(2, 2));
    }

    [Fact]
    public void MapAssetDocument_Should_Resize_And_Preserve_Existing_Tiles()
    {
        var document = MapAssetDocument.CreateDefault();
        document.SetTile(1, 1, '*');

        document.Resize(20, 9);

        Assert.Equal(20, document.Width);
        Assert.Equal(9, document.Height);
        Assert.Equal('*', document.GetTile(1, 1));
        Assert.Equal('.', document.GetTile(19, 8));
    }

    [Fact]
    public void MapAssetDocument_Should_Resize_And_Crop_Existing_Tiles()
    {
        var document = MapAssetDocument.CreateDefault();

        document.Resize(4, 3);

        Assert.Equal(4, document.Width);
        Assert.Equal(3, document.Height);
        Assert.Equal("####", document.Tiles[0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => document.GetTile(4, 0));
    }

    [Fact]
    public void MapAssetDocument_Should_Flood_Fill_Contiguous_Region()
    {
        var document = MapAssetDocument.CreateDefault();

        document.FloodFill(1, 1, '*');

        Assert.Equal('*', document.GetTile(1, 1));
        Assert.Equal('*', document.GetTile(10, 1));
        Assert.Equal('#', document.GetTile(0, 0));
    }

    [Fact]
    public void MapAssetDocument_Should_Not_Flood_Fill_Player_Spawn()
    {
        var document = MapAssetDocument.CreateDefault();

        document.FloodFill(1, 1, '@');

        Assert.Equal('@', document.GetTile(1, 1));
        Assert.Equal('.', document.GetTile(2, 1));
        Assert.Equal('.', document.GetTile(4, 3));
    }

    [Fact]
    public void MapAssetDocument_Should_Reject_Map_Exceeding_Max_Dimension()
    {
        var document = new MapAssetDocument
        {
            Name = "oversized",
            Width = MapAssetDocument.MaxDimension + 1,
            Height = 1,
            Tiles = [new string('.', MapAssetDocument.MaxDimension + 1)]
        };

        Assert.Throws<InvalidDataException>(() => document.Validate());
    }

    [Fact]
    public void MapAssetDocument_Should_Accept_Map_At_Max_Dimension()
    {
        var row = new string('.', MapAssetDocument.MaxDimension);
        var document = new MapAssetDocument
        {
            Name = "max_size",
            Width = MapAssetDocument.MaxDimension,
            Height = 1,
            Tiles = [row]
        };

        var ex = Record.Exception(() => document.Validate());
        Assert.Null(ex);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
