using Terminalia.Assets.Json;

namespace Terminalia.Assets.Json.Tests;

public sealed class JsonAssetLoaderTests : IDisposable
{
    private readonly string _assetRoot;

    public JsonAssetLoaderTests()
    {
        _assetRoot = Path.Combine(Path.GetTempPath(), $"terminalia-assets-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_assetRoot, "maps"));
        Directory.CreateDirectory(Path.Combine(_assetRoot, "sprites"));
        Directory.CreateDirectory(Path.Combine(_assetRoot, "animations"));
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

    [Fact]
    public void JsonAssetLoader_Should_Load_Sprite()
    {
        WriteFile("sprites", "player_idle.txt", """
             O
            /|\
            / \
            """);

        var loader = new JsonAssetLoader(_assetRoot);

        var sprite = loader.LoadSprite("player_idle");

        Assert.Equal("player_idle", sprite.Name);
        Assert.Equal(3, sprite.Lines.Count);
    }

    [Fact]
    public void JsonAssetLoader_Should_Load_Animation()
    {
        WriteFile("animations", "player_walk.json", """
            {
              "name": "player_walk",
              "frameDurationMs": 150,
              "frames": [
                [" O ", "/|\\", "/ \\"],
                [" O ", "\\|/", "/ \\"]
              ]
            }
            """);

        var loader = new JsonAssetLoader(_assetRoot);

        var animation = loader.LoadAnimation("player_walk");

        Assert.Equal("player_walk", animation.Name);
        Assert.Equal(TimeSpan.FromMilliseconds(150), animation.FrameDuration);
        Assert.Equal(2, animation.Frames.Count);
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

    [Theory]
    [InlineData("../../secret")]
    [InlineData("/absolute/path")]
    [InlineData("name/with/slash")]
    public void JsonAssetLoader_Should_Reject_Path_Traversal_In_LoadSprite(string maliciousName)
    {
        var loader = new JsonAssetLoader(_assetRoot);
        Assert.Throws<ArgumentException>(() => loader.LoadSprite(maliciousName));
    }

    [Theory]
    [InlineData("../../secret")]
    [InlineData("/absolute/path")]
    [InlineData("name/with/slash")]
    public void JsonAssetLoader_Should_Reject_Path_Traversal_In_LoadAnimation(string maliciousName)
    {
        var loader = new JsonAssetLoader(_assetRoot);
        Assert.Throws<ArgumentException>(() => loader.LoadAnimation(maliciousName));
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
