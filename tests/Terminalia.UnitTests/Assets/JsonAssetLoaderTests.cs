using Terminalia.Assets.Json;

namespace Terminalia.UnitTests.Assets;

public sealed class JsonAssetLoaderTests : IDisposable
{
    private readonly string _assetRoot;

    public JsonAssetLoaderTests()
    {
        _assetRoot = Path.Combine(Path.GetTempPath(), $"terminalia-assets-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_assetRoot, "maps"));
    }

    [Fact]
    public void JsonAssetLoader_Should_Load_Map()
    {
        WriteFile("maps", "first_dungeon.json", """
            {
              "name": "first_dungeon",
              "width": 4,
              "height": 3,
              "tiles": [
                "####",
                "#@.#",
                "####"
              ],
              "legend": {
                "#": "wall",
                ".": "floor",
                "@": "player_spawn"
              }
            }
            """);

        var loader = new JsonAssetLoader(_assetRoot);

        var map = loader.LoadMap("first_dungeon");

        Assert.Equal("first_dungeon", map.Name);
        Assert.Equal(4, map.Width);
        Assert.Equal(3, map.Height);
        Assert.Equal("player_spawn", map.Legend['@']);
    }

    [Fact]
    public void JsonAssetLoader_Should_Reject_Invalid_Map()
    {
        WriteFile("maps", "bad_map.json", """
            {
              "name": "bad_map",
              "width": 4,
              "height": 2,
              "tiles": [
                "####",
                "#@.#",
                "####"
              ],
              "legend": {
                "#": "wall"
              }
            }
            """);

        var loader = new JsonAssetLoader(_assetRoot);

        Assert.Throws<InvalidDataException>(() => loader.LoadMap("bad_map"));
    }

    [Theory]
    [InlineData("../../secret")]
    [InlineData("../maps/other")]
    [InlineData("/absolute/path")]
    [InlineData("name/with/slash")]
    [InlineData("name\\with\\backslash")]
    public void JsonAssetLoader_Should_Reject_Path_Traversal_In_LoadMap(string maliciousName)
    {
        var loader = new JsonAssetLoader(_assetRoot);
        Assert.Throws<ArgumentException>(() => loader.LoadMap(maliciousName));
    }

    public void Dispose()
    {
        if (Directory.Exists(_assetRoot))
        {
            Directory.Delete(_assetRoot, recursive: true);
        }
    }

    private void WriteFile(string folder, string fileName, string content)
    {
        File.WriteAllText(Path.Combine(_assetRoot, folder, fileName), content);
    }
}
