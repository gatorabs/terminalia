using Terminalia.Assets.Json;
using Terminalia.Core.Assets;
using Terminalia.Core.ValueObjects;

namespace Terminalia.UnitTests.Assets;

public sealed class MapAssetDocumentEntitiesTests : IDisposable
{
    private readonly string _tempDirectory;

    public MapAssetDocumentEntitiesTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"terminalia-map-entities-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void Should_Set_And_Get_Entity_Property()
    {
        var document = MapAssetDocument.CreateDefault();

        document.SetEntityProperty(4, 3, EntityPropertyKeys.Speed, "3");

        Assert.Equal("3", document.GetEntityProperty(4, 3, EntityPropertyKeys.Speed));
        Assert.Null(document.GetEntityProperty(4, 3, EntityPropertyKeys.Health));
        Assert.Null(document.GetEntityProperty(1, 1, EntityPropertyKeys.Speed));
    }

    [Fact]
    public void Should_Clear_Properties_When_Tile_Is_Repainted()
    {
        var document = MapAssetDocument.CreateDefault();
        document.SetEntityProperty(11, 2, EntityPropertyKeys.Collectible, "false");

        document.SetTile(11, 2, '.');

        Assert.Null(document.GetEntityProperty(11, 2, EntityPropertyKeys.Collectible));
    }

    [Fact]
    public void Should_Migrate_Player_Properties_When_Spawn_Moves()
    {
        var document = MapAssetDocument.CreateDefault();
        document.SetEntityProperty(4, 3, EntityPropertyKeys.Speed, "2");

        document.SetTile(8, 4, '@');

        Assert.Equal("2", document.GetEntityProperty(8, 4, EntityPropertyKeys.Speed));
        Assert.Null(document.GetEntityProperty(4, 3, EntityPropertyKeys.Speed));
    }

    [Fact]
    public void Should_Drop_Out_Of_Bounds_Properties_On_Resize()
    {
        var document = MapAssetDocument.CreateDefault();
        document.SetEntityProperty(11, 2, EntityPropertyKeys.Collectible, "false");
        document.SetEntityProperty(1, 1, EntityPropertyKeys.Health, "5");

        document.Resize(6, 6);

        Assert.Null(document.GetEntityProperty(11, 2, EntityPropertyKeys.Collectible));
        Assert.Equal("5", document.GetEntityProperty(1, 1, EntityPropertyKeys.Health));
    }

    [Fact]
    public void Should_Round_Trip_Entity_Properties_Through_Save_And_Load()
    {
        var path = Path.Combine(_tempDirectory, "entities_map.json");
        var document = MapAssetDocument.CreateDefault();
        document.SetEntityProperty(4, 3, EntityPropertyKeys.Speed, "4");
        document.SetEntityProperty(11, 2, EntityPropertyKeys.DisappearOnCollect, "false");

        document.Save(path);
        var loaded = MapAssetDocument.Load(path);

        Assert.Equal("4", loaded.GetEntityProperty(4, 3, EntityPropertyKeys.Speed));
        Assert.Equal("false", loaded.GetEntityProperty(11, 2, EntityPropertyKeys.DisappearOnCollect));
    }

    [Fact]
    public void Should_Expose_Entity_Properties_Through_TileMap()
    {
        var document = MapAssetDocument.CreateDefault();
        document.SetEntityProperty(4, 3, EntityPropertyKeys.Speed, "2");

        var map = document.ToTileMap();

        Assert.Equal("2", map.GetEntityProperties(4, 3)[EntityPropertyKeys.Speed]);
        Assert.Empty(map.GetEntityProperties(1, 1));
        Assert.Single(document.ToEntityProperties(), pair => pair.Key == new Position(4, 3));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
